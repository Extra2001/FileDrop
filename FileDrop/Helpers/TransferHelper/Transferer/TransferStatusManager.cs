using FileDrop.Helpers.Dialog;
using System;
using FileDrop.Models.Database;

namespace FileDrop.Helpers.TransferHelper.Transferer
{
    public class TransferStatusManager
    {
        public static TransferStatusManager manager { get; private set; }
        public int status = 1;
        public Transfer transfer;

        public static TransferStatusManager StartNew(Transfer transferInfo)
        {
            manager = new TransferStatusManager()
            {
                transfer = transferInfo
            };

            ModelDialog.ShowWaiting("正在发送文件", $"正在发送{transferInfo.FileInfos.Count}个文件");
            return manager;
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
            _ = ModelDialog.ShowDialog("发送完成", $"共发送了{transfer.FileInfos.Count}个文件");
        }

        public void ReportError(bool fatal, string message)
        {
            _ = ModelDialog.ShowDialog("发送错误", message);
            status = 2;
        }
    }
}
