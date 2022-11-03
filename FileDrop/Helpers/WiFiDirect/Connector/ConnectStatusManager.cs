using FileDrop.Helpers.Dialog;
using FileDrop.Helpers.WiFiDirect.Advertiser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileDrop.Helpers.WiFiDirect.Connector
{
    public static class ConnectStatusManager
    {
        public static void ReportProgress(string message)
        {
            ModelDialog.ShowWaiting("连接中", message);
        }

        public static void ReportError(bool isFatal, string message)
        {
            _ = ModelDialog.ShowDialog("发送失败", message);
            WiFiDirectConnector.StopWatcher();
            WiFiDirectConnector.StartWatcher();
        }
    }
}
