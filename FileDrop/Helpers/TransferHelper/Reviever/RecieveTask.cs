using FileDrop.Helpers.Dialog;
using FileDrop.Helpers.TransferHelper.Reciever;
using FileDrop.Models;
using FileDrop.Models.Database;
using FileDrop.Models.Transfer;
using FluentFTP;
using Microsoft.UI.Xaml.Controls;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Text;
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
        public static void WaitForTransfer(EndpointPair endpointPair)
        {
            NetworkHelper.SetNetworkProfileToPrivate();
            server.SafeDispose();

            server = new TcpService();
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
                        var respond = new TransferRespond() { Recieve = true };
                        server.SendAsync(client.ID, JsonConvert.SerializeObject(respond));
                        _ = StartRecieve(endpointPair.RemoteHostName.DisplayName,
                                transferInfo.port, transferInfo.token, transferInfo);
                    }
                    else
                    {
                        var respond = new TransferRespond() { Recieve = false };
                        server.SendAsync(client.ID, JsonConvert.SerializeObject(respond));
                    }
                });
            };
            server.Setup(new TouchSocketConfig()
                .SetListenIPHosts(new IPHost[] {
                        new IPHost(endpointPair.LocalHostName.DisplayName + ":" + 31826) }))
                .Start();
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

            ftpClient = new AsyncFtpClient(host, "root", token, port);
            await ftpClient.AutoConnect();

            RecieveStatusManager.StartNew(transfer);

            var files = transferInfo.TransferInfos.Select(x => x.InPackagePath);
            var results = await ftpClient.DownloadDirectory(folder, "/", progress: new DownloadProgressManager());

            RecieveStatusManager.manager.ReportDone(results);
        }
        public static void RecieveDone()
        {
            try
            {
                ftpClient.AutoDispose();
                server.Send(server.GetClients().FirstOrDefault().ID, "RecieveDone");
                server.Clear();
                server.Stop();
                server.SafeDispose();
                server = null;
            }
            catch { }
        }
    }
}
