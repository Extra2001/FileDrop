using FileDrop.Helpers.Dialog;
using FileDrop.Models;
using FileDrop.Models.Transfer;
using Microsoft.UI.Xaml.Controls;
using Newtonsoft.Json;
using System;
using System.Net.Sockets;
using System.Text;
using TouchSocket.Core.ByteManager;
using TouchSocket.Sockets;

namespace FileDrop.Helpers.TransferHelper.Reviever
{
    public class InfoSocketServer : TcpService
    {
        protected override void OnReceived(SocketClient socketClient, ByteBlock byteBlock, IRequestInfo requestInfo)
        {
            string mes = Encoding.UTF8.GetString(byteBlock.Buffer, 0, byteBlock.Len);
            var transferInfo = JsonConvert.DeserializeObject<TransferInfo>(mes);
            App.mainWindow.DispatcherQueue.TryEnqueue(async () =>
            {
                var dres = await ModelDialog.ShowDialog("开始传输",
                $"{transferInfo.deviceName}想要共享{transferInfo.FileInfos.Count}个文件（夹）", "接受", "取消");

                if (dres == ContentDialogResult.Primary)
                {
                    var respond = new TransferRespond()
                    {
                        Recieve = true,
                        Port = NetworkHelper.GetRandomPort(),
                        Token = Guid.NewGuid().ToString()
                    };
                    this.SendAsync(socketClient.ID, JsonConvert.SerializeObject(respond));
                }
                else
                {
                    var respond = new TransferRespond() { Recieve = false };
                    this.SendAsync(socketClient.ID, JsonConvert.SerializeObject(respond));
                }
            });
            base.OnReceived(socketClient, byteBlock, requestInfo);
        }
    }
}
