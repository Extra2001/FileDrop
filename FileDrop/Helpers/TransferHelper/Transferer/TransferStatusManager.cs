using FileDrop.Helpers.Dialog;
using FileDrop.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TouchSocket.Core.Run;
using TouchSocket.Core;
using TouchSocket.Rpc.TouchRpc;
using FileDrop.Helpers.TransferHelper.Reciever;
using FileDrop.Models.Database;

namespace FileDrop.Helpers.TransferHelper.Transferer
{
    public class TransferStatusManager
    {
        public static TransferStatusManager manager { get; private set; }
        public int status = 1;
        public Transfer transfer;
        public List<FileOperator> fileOperators;

        public static TransferStatusManager StartNew(Transfer transferInfo, List<FileOperator> fileOperators)
        {
            manager = new TransferStatusManager()
            {
                transfer = transferInfo,
                fileOperators = fileOperators
            };

            ModelDialog.ShowWaiting("正在发送文件", $"正在发送{transferInfo.FileInfos.Count}个文件");

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

            progress /= fileOperators.Count;

            if (finished || status == 0)
            {
                ReportDone();
                loop.Dispose();
            }
            else
            {
                ModelDialog.ShowWaiting
                ("正在发送文件", $"进度：{progress * 100}%，速度：{SpeedParser.Parse(speedSum)}");
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
            _ = ModelDialog.ShowDialog("发送完成", $"共发送了{transfer.FileInfos.Count}个文件");
        }

        public void ReportError(bool fatal, string message)
        {
            _ = ModelDialog.ShowDialog("发送错误", message);
            status = 2;
        }
    }
}
