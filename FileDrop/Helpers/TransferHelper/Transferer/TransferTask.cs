using FileDrop.Helpers.Dialog;
using FileDrop.Helpers.TransferHelper.Reciever;
using FileDrop.Helpers.WiFiDirect;
using FileDrop.Helpers.WiFiDirect.Connector;
using FileDrop.Models;
using FileDrop.Models.Database;
using FileDrop.Models.Transfer;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Shapes;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TouchSocket.Core;
using TouchSocket.Core.Config;
using TouchSocket.Core.Dependency;
using TouchSocket.Core.Log;
using TouchSocket.Core.Plugins;
using TouchSocket.Core.Run;
using TouchSocket.Http;
using TouchSocket.Rpc.TouchRpc;
using TouchSocket.Sockets;
using Windows.Networking;
using Windows.Storage;
using Windows.Storage.Streams;
using WindowsFirewallHelper;

namespace FileDrop.Helpers.TransferHelper.Transferer
{
    public static class TransferTask
    {
        private static HttpService service = null;
        public static void RequestTransfer(EndpointPair endpointPair, TransferInfo transferInfo)
        {
            TcpClient tcpClient = new TcpClient();
            tcpClient.Connected += (client, e) =>
            {
                tcpClient.Send(JsonConvert.SerializeObject(transferInfo));
            };
            tcpClient.Received += (client, byteBlock, requestInfo) =>
            {
                string mes = Encoding.UTF8.GetString(byteBlock.Buffer, 0, byteBlock.Len);
                if (mes == "RecieveDone")
                {
                    TransferStatusManager.manager.ReportDone();
                    tcpClient.SafeDispose();
                    service.SafeDispose();
                    service = null;
                    tcpClient = null;
                }
                else
                {
                    var respond = JsonConvert.DeserializeObject<TransferRespond>(mes);
                    if (respond.Recieve)
                    {
                        StartTransfer($"{endpointPair.LocalHostName.DisplayName}:{respond.Port}",
                            respond.Token, transferInfo);
                    }
                }
            };

            TouchSocketConfig config = new TouchSocketConfig();
            config.SetRemoteIPHost(new IPHost(endpointPair.RemoteHostName.DisplayName + ":31826"));
            tcpClient.Setup(config);
            int retry = 0;
            retryConnect:
            try
            {
                tcpClient.Connect();
            }
            catch
            {
                if (retry > 3)
                    ConnectStatusManager.ReportError(true, "连接超时");
                else
                {
                    retry++;
                    goto retryConnect;
                }
            }
        }
        private static void StartTransfer(string localIPHost, string token, TransferInfo transferInfo)
        {
            var transfer = new Transfer()
            {
                DirectoryName = null,
                StartTime = DateTimeOffset.Now,
                TransferInfos = transferInfo.TransferInfos,
                FileInfos = transferInfo.FileInfos,
                From = null,
                To = transferInfo.deviceName,
                TransferDirection = TransferDirection.Transfer
            };

            TransferStatusManager.StartNew(transfer);

            service.SafeDispose();
            service = new HttpService();
            var config = new TouchSocketConfig();
            var plugin = new TransferHttpPlugin();

            foreach (var item in transferInfo.TransferInfos)
                plugin.AddFile(item.Path, item.InPackagePath);

            config.UsePlugin()
                .SetListenIPHosts(new IPHost[] { new IPHost(localIPHost) })
                .ConfigurePlugins(a =>
                {
                    a.Add(plugin);
                });

            service.Setup(config).Start();
        }
    }
}
