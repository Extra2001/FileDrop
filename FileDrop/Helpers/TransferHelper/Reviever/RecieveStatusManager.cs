using FileDrop.Helpers.Dialog;
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
        public List<FileOperator> fileOperators;

        public static RecieveStatusManager StartNew(Transfer transferInfo, List<FileOperator> fileOperators)
        {
            manager = new RecieveStatusManager()
            {
                transfer = transferInfo
            };

            ModelDialog.ShowWaiting("正在接收文件", $"正在接收{transferInfo.FileInfos.Count}个文件");
            manager.fileOperators = fileOperators;

            LoopAction.CreateLoopAction(-1, 1000, manager.ReportProgressSpeed)
                .RunAsync();
            return manager;
        }

        public void ReportProgressSpeed(LoopAction loop)
        {
            bool finished = true;
            long speedSum = 0;
            float progress = 0;
            foreach (var item in fileOperators)
            {
                if (item.Result.ResultCode == ResultCode.Default)
                    finished = false;
                speedSum += item.Speed();
                progress += item.Progress;
            }

            progress /= transfer.TransferInfos.Count;

            if ((finished && fileOperators.Count != 0) || status == 0)
            {
                loop.Dispose();
            }
            else
            {
                ModelDialog.ShowWaiting
                ("正在接收文件", $"进度：{progress * 100}%，速度：{SpeedParser.Parse(speedSum)}");
            }
        }

        public void ReportDone()
        {
            status = 0;
            transfer.EndTime = DateTimeOffset.Now;
            var collection = Repo.database.GetCollection<Transfer>();
            collection.Insert(transfer);
            _ = ModelDialog.ShowDialog("接收完成", $"共接收了{transfer.FileInfos.Count}个文件");
            _ = Launcher.LaunchFolderPathAsync(transfer.DirectoryName);
        }

        public void ReportError(string message)
        {
            _ = ModelDialog.ShowDialog("接收错误", message);
            status = 2;
        }
    }
}
