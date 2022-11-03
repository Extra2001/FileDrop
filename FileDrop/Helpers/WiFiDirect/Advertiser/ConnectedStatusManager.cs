using FileDrop.Helpers.Dialog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileDrop.Helpers.WiFiDirect.Advertiser
{
    public static class ConnectedStatusManager
    {
        public static void ReportProgress(string message)
        {
            ModelDialog.ShowWaiting("连接中", message);
        }

        public static void ReportError(bool isFatal, string message)
        {
            _ = ModelDialog.ShowDialog("接收失败", message);
            WiFiDirectAdvertiser.StopAdvertisement();
            WiFiDirectAdvertiser.StartAdvertisement();
        }
    }
}
