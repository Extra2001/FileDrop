using FileDrop.Helpers.Dialog;
using FileDrop.Helpers.TransferHelper.Reviever;
using FileDrop.Models.Database;
using FluentFTP;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
            RestoreFile.RestoreZip(Path.Combine(transfer.DirectoryName, "package.zip"));
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
            RecieveTask.RecieveDone();
            status = 2;
        }
    }
}
