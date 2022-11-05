using FileDrop.Helpers.TransferHelper.Transferer.FileSystem;
using FileDrop.Helpers.WiFiDirect.Connector;
using FileDrop.Models;
using FileDrop.Models.Database;
using FileDrop.Models.Transfer;
using FubarDev.FtpServer;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.Text;
using System.Threading;
using TouchSocket.Core.Config;
using TouchSocket.Sockets;
using Windows.Networking;

namespace FileDrop.Helpers.TransferHelper.Transferer
{
    public static class TransferTask
    {
        private static IFtpServerHost service = null;
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
                    service.StopAsync().Wait();
                    service = null;
                    tcpClient = null;
                }
                else
                {
                    var respond = JsonConvert.DeserializeObject<TransferRespond>(mes);
                    if (respond.Recieve)
                    {
                        StartTransfer(respond.Port, respond.Token, transferInfo);
                    }
                }
            };

            TouchSocketConfig config = new TouchSocketConfig();
            config.SetRemoteIPHost(new IPHost(endpointPair.RemoteHostName.DisplayName + ":31826"));
            tcpClient.Setup(config);
            bool success = false;
            for (int retry = 0; retry < 3; retry++)
            {
                try
                {
                    tcpClient.Connect();
                    success = true;
                    break;
                }
                catch { }
            }
            if (!success) ConnectStatusManager.ReportError(true, "连接超时");
        }
        private static void StartTransfer(int port, string token, TransferInfo transferInfo)
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

            service = CreateFTPServer(port, token, transferInfo);
        }

        public static IFtpServerHost CreateFTPServer(int port,string token, TransferInfo transferInfo)
        {
            var services = new ServiceCollection();

            services.Configure<FTPFileSystemOptions>(opt => 
                opt.transferInfo = transferInfo);

            services.AddFtpServer(builder =>
            {
                builder.UseFTPFileSystem();
            });

            services.AddFtpToken(token);

            services.Configure<FtpServerOptions>(options =>
            {
                options.Port = port;
            });

            using (var serviceProvider = services.BuildServiceProvider())
            {
                var ftpServerHost = serviceProvider.GetRequiredService<IFtpServerHost>();
                ftpServerHost.StartAsync(CancellationToken.None);
                return ftpServerHost;
            }
        }
    }
}
