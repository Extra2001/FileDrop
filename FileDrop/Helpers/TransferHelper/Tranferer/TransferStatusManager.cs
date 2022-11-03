using FileDrop.Helpers.Dialog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileDrop.Helpers.TransferHelper.Tranferer
{
    public class TransferStatusManager
    {
        public static TransferStatusManager manager { get; private set; }
        public int status = 1;
        public int itemCount { get; set; }

        public static TransferStatusManager StartNew(int itemCount)
        {
            manager = new TransferStatusManager()
            {
                itemCount = itemCount
            };
            return manager;
        }

        public void ReportTransferItem(int index)
        {
            ModelDialog.ShowWaiting("正在发送", $"正在发送第{index + 1}个文件，共{itemCount}个");
        }

        public void ReportPack(int index, int count)
        {

        }

        public void ReportDone()
        {
            _ = ModelDialog.ShowDialog("发送完成", $"共发送了{itemCount}个文件");
            status = 0;
        }

        public void ReportError(bool fatal, string message)
        {
            _ = ModelDialog.ShowDialog("发送错误", message);
            status = 2;
        }
    }
}
