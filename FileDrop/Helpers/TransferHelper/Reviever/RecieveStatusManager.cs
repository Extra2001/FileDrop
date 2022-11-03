using FileDrop.Helpers.Dialog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace FileDrop.Helpers.TransferHelper.Reciever
{
    public class RecieveStatusManager
    {
        public static RecieveStatusManager manager { get; private set; }
        public int status = 1;
        public int itemCount { get; set; }

        public static RecieveStatusManager StartNew(int itemCount)
        {
            manager = new RecieveStatusManager()
            {
                itemCount = itemCount
            };
            return manager;
        }

        public void ReportTransferItem(int index)
        {
            ModelDialog.ShowWaiting("正在接收", $"正在接收第{index + 1}个文件，共{itemCount}个");
        }

        public void ReportPack(int index, int count)
        {

        }

        public void ReportDone()
        {
            _ = ModelDialog.ShowDialog("接收完成", $"共接收了{itemCount}个文件");
            status = 0;
        }

        public void ReportError(bool fatal, string message)
        {
            _ = ModelDialog.ShowDialog("接收错误", message);
            status = 2;
        }
    }
}
