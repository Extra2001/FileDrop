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
using TouchSocket.Core.Run;
using TouchSocket.Rpc.TouchRpc;
using TouchSocket.Sockets;
using Windows.Networking;
using Windows.Storage;
using Windows.Storage.Streams;

namespace FileDrop.Helpers.TransferHelper.Transferer
{
    public static class TransferTask
    {
        public static void RequestTransfer(HostName host, TransferInfo transferInfo)
        {
            TcpClient tcpClient = new TcpClient();
            tcpClient.Connected += (client, e) =>
            {
                tcpClient.Send(JsonConvert.SerializeObject(transferInfo));
            };
            tcpClient.Disconnected += (client, e) =>
            {

            };
            tcpClient.Received += (client, byteBlock, requestInfo) =>
            {
                string mes = Encoding.UTF8.GetString(byteBlock.Buffer, 0, byteBlock.Len);
                var respond = JsonConvert.DeserializeObject<TransferRespond>(mes);
                if (respond.Recieve)
                {
                    StartTransfer($"{host.DisplayName}:{respond.Port}", respond.Token, transferInfo);
                }
            };

            TouchSocketConfig config = new TouchSocketConfig();
            config.SetRemoteIPHost(new IPHost(host.DisplayName + ":31826"));
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

        private static async void StartTransfer(string remoteIPHost, string token, TransferInfo transferInfo)
        {
            await Task.Run(() =>
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

                TcpTouchRpcClient client = new TouchSocketConfig()
                .SetRemoteIPHost(remoteIPHost)
                .SetVerifyToken(token)
                .UsePlugin()
                .ConfigurePlugins(a =>
                {
                    a.Add(new TransferPlugin());
                })
                .BuildWithTcpTouchRpcClient();

                List<FileRequest> requests = new List<FileRequest>();
                List<FileOperator> fileOperators = new List<FileOperator>();
                List<Metadata> metadatas = new List<Metadata>();

                foreach (var item in transferInfo.TransferInfos)
                {
                    FileRequest fileRequest = new FileRequest()
                    {
                        Path = item.Path,
                        SavePath = item.InPackagePath
                    };
                    requests.Add(fileRequest);
                    if (item.TransferType == Models.TransferType.Zip)
                        metadatas.Add(new Metadata().Add("Zip", "true"));
                    else metadatas.Add(null);
                    var fo = new FileOperator();
                    fo.Timeout = -1;
                    fileOperators.Add(fo);
                }

                TransferStatusManager.StartNew(transfer, fileOperators);

                client.PushFiles(10, requests.ToArray(), fileOperators.ToArray(), metadatas.ToArray());
                client.Close();

                TransferStatusManager.manager.ReportDone();
            });
        }
    }
}
