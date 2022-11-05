using FileDrop.Helpers.Dialog;
using FileDrop.Helpers.TransferHelper.Reviever;
using FileDrop.Helpers.TransferHelper.Transferer;
using FileDrop.Models;
using FileDrop.Models.Database;
using FluentFTP;
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

        public static RecieveStatusManager StartNew(Transfer transferInfo)
        {
            manager = new RecieveStatusManager()
            {
                transfer = transferInfo
            };

            ModelDialog.ShowWaiting("正在接收文件", $"正在接收{transferInfo.FileInfos.Count}个文件");
            return manager;
        }

        public void ReportDone(List<FtpResult> results)
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
            int success = results.Where(x => x.IsSuccess).Count();
            var failed = results.Where(x => x.IsFailed).Select(x => x.Exception.Message);
            var error = string.Join('|', failed);
            _ = ModelDialog.ShowDialog("接收完成", $"成功接收{success}个文件，{failed.Count()}项失败，原因：{error}");
            _ = Launcher.LaunchFolderPathAsync(transfer.DirectoryName);
        }

        public void ReportError(string message)
        {
            _ = ModelDialog.ShowDialog("接收错误", message);
            status = 2;
        }
    }
}
