using FileDrop.Helpers.Dialog;
using FileDrop.Helpers.WiFiDirect.Connector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.WiFiDirect;
using Windows.Networking;
using Windows.Networking.Sockets;

namespace FileDrop.Helpers.WiFiDirect.Connector
{
    /// <summary>
    /// 主动连接的设备
    /// </summary>
    public class L2ConnectDevice : IDisposable
    {
        public WiFiDirectDevice WfdDevice { get; }
        private HostName remoteHostName { get; }
        private SocketReaderWriter SocketRW { get; set; }
        public L2ConnectDevice(WiFiDirectDevice wfdDevice)
        {
            WfdDevice = wfdDevice;
            IReadOnlyList<EndpointPair> endpointPairs = wfdDevice.GetConnectionEndpointPairs();
            remoteHostName = endpointPairs[0].RemoteHostName;
        }
        public async Task<SocketReaderWriter> EstablishSocket()
        {
            try
            {
                ConnectStatusManager.ReportProgress("正在发起L4连接请求");
                StreamSocket clientSocket = new StreamSocket();
                await clientSocket.ConnectAsync(remoteHostName, ConnectDefinition.strServerPort);
                SocketRW = new SocketReaderWriter(clientSocket);
                ConnectStatusManager.ReportProgress("L4连接建立成功");
                return SocketRW;
            }
            catch (Exception ex)
            {
                ConnectStatusManager.ReportError(true, ex.Message);
                return null;
            }
        }
        public void Dispose()
        {
            SocketRW?.Dispose();
            WfdDevice.Dispose();
        }
    }
}
