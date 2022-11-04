using FileDrop.Helpers.TransferHelper.Reciever;
using FileDrop.Models;
using FileDrop.Models.Database;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System;
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
        private static InfoSocketServer server = null;
        public static void WaitForTransfer(HostName localHostName)
        {
            NetworkHelper.SetNetworkProfileToPrivate();
            if (server != null)
            {
                server = new InfoSocketServer();
                server.Setup(new TouchSocketConfig()
                    .SetListenIPHosts(new IPHost[] { new IPHost(localHostName.DisplayName + ":" + 31826) })
                    .SetMaxCount(10000)
                    .SetThreadCount(10))
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
