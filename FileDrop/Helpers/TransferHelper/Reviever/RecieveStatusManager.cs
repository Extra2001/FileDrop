using Downloader;
using FileDrop.Helpers.Dialog;
using FileDrop.Helpers.TransferHelper.Reviever;
using FileDrop.Helpers.TransferHelper.Transferer;
using FileDrop.Models;
using FileDrop.Models.Database;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using TouchSocket.Core;
using TouchSocket.Core.Run;
using TouchSocket.Rpc.TouchRpc;
using Windows.System;

namespace FileDrop.Helpers.TransferHelper.Reciever
{
    public class RecieveStatusManager
    {
        public static RecieveStatusManager manager { get; private set; }
        public int status = 1;
        public Transfer transfer;
        public List<DownloadProgress> downloaders;

        public static RecieveStatusManager StartNew(Transfer transferInfo, List<DownloadService> downloaders)
        {
            manager = new RecieveStatusManager()
            {
                transfer = transferInfo
            };

            ModelDialog.ShowWaiting("正在接收文件", $"正在接收{transferInfo.FileInfos.Count}个文件");

            foreach (var item in downloaders)
            {
                item.DownloadFileCompleted += manager.Item_DownloadFileCompleted;
                item.DownloadProgressChanged += manager.Item_DownloadProgressChanged;
                manager.downloaders.Add(new DownloadProgress(item));
            }

            LoopAction.CreateLoopAction(-1, 1000, manager.ReportProgress);
            return manager;
        }
        private void Item_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            var find = downloaders.Where(x => x.downloader == sender as DownloadService)
                .FirstOrDefault();
            find.progressPercentage = e.ProgressPercentage;
        }
        private void Item_DownloadFileCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            var find = downloaders.Where(x => x.downloader == sender as DownloadService)
                .FirstOrDefault();
            find.completed = true;
        }
        private void ReportProgress(LoopAction loop)
        {
            if (downloaders.Where(x => !x.completed).Any())
            {
                double progress = 0;
                foreach (var item in downloaders)
                    progress += item.progressPercentage;
                progress /= downloaders.Count;
                ModelDialog.ShowWaiting("正在接收文件", $"进度：{progress}%");
            }
            else
            {
                loop.SafeDispose();
                ReportDone();
            }
        }
        public void ReportDone()
        {
            status = 0;
            try
            {
                transfer.EndTime = DateTimeOffset.Now;
                var collection = Repo.database.GetCollection<Transfer>();
                collection.Insert(transfer);
            }
            catch { }
            RecieveTask.RecieveDone();
            _ = ModelDialog.ShowDialog("接收完成", $"共接收了{transfer.FileInfos.Count}个文件");
            _ = Launcher.LaunchFolderPathAsync(transfer.DirectoryName);
        }
        public void ReportError(string message)
        {
            _ = ModelDialog.ShowDialog("接收错误", message);
            status = 2;
        }

        public class DownloadProgress
        {
            public DownloadService downloader;
            public bool completed = false;
            public double progressPercentage = 0;

            public DownloadProgress(DownloadService service)
            {
                downloader = service;
            }
        }
    }
}
