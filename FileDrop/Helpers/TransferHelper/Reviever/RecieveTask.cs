using FileDrop.Helpers.Dialog;
using FileDrop.Helpers.TransferHelper.Reciever;
using FileDrop.Models;
using FileDrop.Models.Database;
using FileDrop.Models.Transfer;
using FluentFTP;
using Microsoft.UI.Xaml.Controls;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TouchSocket.Core.Config;
using TouchSocket.Sockets;
using Windows.Networking;

namespace FileDrop.Helpers.TransferHelper.Reviever
{
    public static class RecieveTask
    {
        private static TcpService server = null;
        private static AsyncFtpClient ftpClient = null;
        private static Task<string> tryIP = null;
        public static void WaitForTransfer(EndpointPair endpointPair)
        {
            NetworkHelper.SetNetworkProfileToPrivate();
            server.SafeDispose();

            server = new TcpService();

            server.Received += (client, byteBlock, requestInfo) =>
            {
                string mes = Encoding.UTF8.GetString(byteBlock.Buffer, 0, byteBlock.Len);
                var transferInfo = JsonConvert.DeserializeObject<TransferInfo>(mes);
                tryIP = TryIPs(transferInfo.ipAddresses, transferInfo.port, transferInfo.token);

                NotificationDialog.SendRecieveToast($"{transferInfo.deviceName}想要共享{transferInfo.FileInfos.Count}个文件（夹）", res =>
                {
                    App.mainWindow.DispatcherQueue.TryEnqueue(async () =>
                    {
                        if (res == 3)
                        {
                            var dres = await ModelDialog.ShowDialog("开始传输",
                                        $"{transferInfo.deviceName}想要共享" +
                                        $"{transferInfo.FileInfos.Count}个文件（夹）", "接受", "取消");
                            if (dres == ContentDialogResult.Primary)
                                Accept(client, endpointPair, transferInfo);
                            else Decline(client);
                        }
                        else if (res == 1) Accept(client, endpointPair, transferInfo);
                        else Decline(client);
                    });
                });

            };
            server.Setup(new TouchSocketConfig()
                .SetListenIPHosts(new IPHost[] {
                        new IPHost(endpointPair.LocalHostName.DisplayName + ":" + 31826) }))
                .Start();
        }

        public static async Task<string> TryIPs(IEnumerable<string> ips, int port, string token)
        {
            var clients = new List<AsyncFtpClient>();
            var tasks = new List<Task<FtpProfile>>();
            foreach (var item in ips)
            {
                var client = new AsyncFtpClient(item, "root", token, port);
                clients.Add(client);
                var cancel = new CancellationTokenSource(3000);
                tasks.Add(client.AutoConnect(cancel.Token));
            }
            var datas = tasks.Select(x => x.ContinueWith(y => y));
            var res = (await Task.WhenAll(datas)).AsEnumerable();
            res = res.Where(x => !x.IsCanceled && !x.IsFaulted);
            foreach (var item in res)
            {
                if (item.Result != null && item.Result.Host != null)
                {
                    foreach (var client in clients)
                    {
                        client.AutoDispose();
                    }
                    return item.Result.Host;
                }
            }
            return null;
        }

        public static async void Accept(SocketClient client, EndpointPair endpointPair, TransferInfo transferInfo)
        {
            var respond = new TransferRespond() { Recieve = true };
            server.SendAsync(client.ID, JsonConvert.SerializeObject(respond));
            string host = endpointPair.RemoteHostName.DisplayName;
            if (tryIP != null)
            {
                var str = await tryIP;
                if (str != null) host = str;
            }
            _ = StartRecieve(host, transferInfo.port, transferInfo.token, transferInfo);
        }

        public static void Decline(SocketClient client)
        {
            var respond = new TransferRespond() { Recieve = false };
            server.SendAsync(client.ID, JsonConvert.SerializeObject(respond));
            tryIP = null;
        }

        public static void StopWaitForTransfer()
        {
            server?.Stop();
        }
        public static async Task StartRecieve(string host, int port, string token, TransferInfo transferInfo)
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

            ftpClient = new AsyncFtpClient(host, "root", token, port);
            
            var cancel = new CancellationTokenSource(3000);
            var connectResult = await ftpClient.AutoConnect(cancel.Token);

            if (connectResult != null)
            {
                var files = transferInfo.TransferInfos.Select(x => x.InPackagePath);
                var results = await ftpClient.DownloadDirectory(folder, "/", progress: new DownloadProgressManager());

                RecieveStatusManager.manager.ReportDone(results);
            }
            else
            {
                RecieveStatusManager.manager.ReportError("连接超时");
            }
        }
        public static void RecieveDone()
        {
            try
            {
                server.Send(server.GetClients().FirstOrDefault().ID, "RecieveDone");
                server.Clear();
                server.Stop();
                server.SafeDispose();
                server = null;
                ftpClient.AutoDispose();
                ftpClient = null;
            }
            catch { }
        }
    }
}
