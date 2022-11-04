using FileDrop.Helpers.Dialog;
using FileDrop.Helpers.TransferHelper.Reciever;
using FileDrop.Models;
using FileDrop.Models.Database;
using FileDrop.Models.Transfer;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Newtonsoft.Json;
using System;
using System.Text;
using TouchSocket.Core.Config;
using TouchSocket.Core.Dependency;
using TouchSocket.Core.Log;
using TouchSocket.Core.Plugins;
using TouchSocket.Rpc.TouchRpc;
using TouchSocket.Sockets;
using Windows.Devices.Portable;
using Windows.Networking;

namespace FileDrop.Helpers.TransferHelper.Reviever
{
    public static class RecieveTask
    {
        private static TcpService server = null;
        public static void WaitForTransfer(HostName localHostName)
        {
            NetworkHelper.SetNetworkProfileToPrivate();
            if (server != null)
            {
                server = new TcpService();
                server.Connected += (o, e) =>
                {

                };
                server.Received += (client, byteBlock, requestInfo) =>
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
                            server.SendAsync(client.ID, JsonConvert.SerializeObject(respond));
                        }
                        else
                        {
                            var respond = new TransferRespond() { Recieve = false };
                            server.SendAsync(client.ID, JsonConvert.SerializeObject(respond));
                        }
                    });
                };
                server.Setup(new TouchSocketConfig()
                    .SetListenIPHosts(new IPHost[] { new IPHost(localHostName.DisplayName + ":" + 31826) }))
                    .Start();
            }
        }

        public static void StopWaitForTransfer()
        {
            server?.Stop();
        }

        public static TcpTouchRpcService StartRecieve(int port, string token, TransferInfo transferInfo)
        {
            var folder = DirectoryHelper.GenerateRecieveFolder(transferInfo.deviceName);
            var transfer = new Transfer()
            {
                DirectoryName = folder,
                StartTime = DateTimeOffset.Now,
                TransferInfos = transferInfo.TransferInfos,
                FileInfos = transferInfo.FileInfos,
                From = transferInfo.deviceName,
                To = null,
                TransferDirection = TransferDirection.Recieve
            };

            RecieveStatusManager.StartNew(transfer);
            var service = new TouchSocketConfig()
                .SetListenIPHosts(new IPHost[] { new IPHost(port) })
                .SetRootPath(DirectoryHelper.GenerateRecieveFolder(transferInfo.deviceName))
                .UsePlugin()
                .ConfigurePlugins(a =>
                {
                    a.Add<RecievePlugin>();
                })
                .SetVerifyToken(token)
                .BuildWithTcpTouchRpcService();
            return service;
        }
    }
}
