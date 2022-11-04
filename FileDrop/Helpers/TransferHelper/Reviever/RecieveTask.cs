using Downloader;
using FileDrop.Helpers.Dialog;
using FileDrop.Helpers.TransferHelper.Reciever;
using FileDrop.Helpers.TransferHelper.Transferer;
using FileDrop.Models;
using FileDrop.Models.Database;
using FileDrop.Models.Transfer;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
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
using TouchSocket.Rpc.TouchRpc;
using TouchSocket.Sockets;
using Windows.Devices.Portable;
using Windows.Networking;

namespace FileDrop.Helpers.TransferHelper.Reviever
{
    public static class RecieveTask
    {
        private static TcpService server = null;
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
                        int port = NetworkHelper.GetRandomPort();
                        string token = Guid.NewGuid().ToString();
                        var respond = new TransferRespond()
                        {
                            Recieve = true,
                            Port = port,
                            Token = token
                        };
                        server.SendAsync(client.ID, JsonConvert.SerializeObject(respond));
                        _ = StartRecieve
                            (endpointPair.RemoteHostName.DisplayName + ":" + port, token, transferInfo);
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
        public static async Task StartRecieve(string remoteIPHost, string token, TransferInfo transferInfo)
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

            List<Task> tasks = new List<Task>();
            List<DownloadService> downloaders = new List<DownloadService>();

            foreach (var item in transferInfo.TransferInfos)
            {
                var downloadOpt = new DownloadConfiguration()
                {
                    ChunkCount = 8,
                    ParallelDownload = true
                };
                var downloader = new DownloadService(downloadOpt);
                string file = Path.Combine(folder, item.InPackagePath);
                string url = @$"http://{remoteIPHost}/{item.InPackagePath}";
                tasks.Add(downloader.DownloadFileTaskAsync(url, file));
            }

            RecieveStatusManager.StartNew(transfer, downloaders);

            await Task.WhenAll(tasks);
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
            }
            catch { }
        }
    }
}
