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
            ToastDialog.Show("其他设备正在请求发送：" + message);
        }

        public static void ReportError(bool isFatal, string message)
        {
            ToastDialog.Show("建立连接失败：" + message);
            WiFiDirectAdvertiser.StopAdvertisement();
            WiFiDirectAdvertiser.StartAdvertisement();
        }
    }
}
