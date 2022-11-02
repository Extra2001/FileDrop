using FileDrop.Helpers.Dialog;
using FileDrop.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileDrop.Helpers.WiFiDirect
{
    public static class SocketRead
    {
        public static async void RecieveRead(SocketReaderWriter.SocketRead read)
        {
            TransferInfo transferInfo;
            try
            {
                transferInfo = JsonConvert.DeserializeObject<TransferInfo>(read.info);
                var res = await ModelDialog.ShowDialog("发送文件", $"{transferInfo.deviceName}想要共享{transferInfo.FileInfos.Count}个文件（夹）", "接受", "取消");
                if (res == Microsoft.UI.Xaml.Controls.ContentDialogResult.Primary)
                {
                    var RW = await WiFiDirectAdvertiser.connectedDevice.EstablishSocket();
                    await RW.WriteAsync(new TransferStartRespond() { Recieve = true });
                }
                else
                {
                    var RW = await WiFiDirectAdvertiser.connectedDevice.EstablishSocket();
                    await RW.WriteAsync(new TransferStartRespond() { Recieve = false });
                }
            }
            catch { }
        }

        public static void SendRead(SocketReaderWriter.SocketRead read)
        {
            TransferStartRespond res = null;
            try { res = JsonConvert.DeserializeObject<TransferStartRespond>(read.info); }
            catch { }
            if (res == null) return;
            else if (res.Recieve == false)
            {
                _ = ModelDialog.ShowDialog("取消", "对方取消了传输");
                return;
            }
            StartTransfer();
        }
        public static void StartTransfer()
        {
            ModelDialog.ShowWaiting("请稍后", "正在开始传输...");
        }
    }
}
