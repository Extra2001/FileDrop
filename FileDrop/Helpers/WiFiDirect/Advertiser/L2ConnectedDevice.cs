using FileDrop.Helpers.Dialog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.WiFiDirect;
using Windows.Networking.Sockets;
using Windows.Networking;

namespace FileDrop.Helpers.WiFiDirect.Advertiser
{
    /// <summary>
    /// 被动连接的设备（适用于播发端）
    /// </summary>
    public class L2ConnectedDevice
    {
        public WiFiDirectDevice WfdDevice { get; }
        public StreamSocketListener socketListener { get; }

        private SocketReaderWriter SocketRW { get; set; }
        private HostName remoteHostName { get; }

        public L2ConnectedDevice(WiFiDirectDevice wfdDevice)
        {
            WfdDevice = wfdDevice;
            IReadOnlyList<EndpointPair> endpointPairs = wfdDevice.GetConnectionEndpointPairs();
            remoteHostName = endpointPairs[0].LocalHostName;

            socketListener = new StreamSocketListener();
            socketListener.ConnectionReceived += SocketListener_ConnectionReceived;
            _ = socketListener.BindEndpointAsync(remoteHostName, ConnectDefinition.strServerPort);
        }
        private void SocketListener_ConnectionReceived(StreamSocketListener sender, StreamSocketListenerConnectionReceivedEventArgs args)
        {
            SocketRW = new SocketReaderWriter(args.Socket);
        }
        public async Task<SocketReaderWriter> EstablishSocket()
        {
            int retry = 0;
            while (SocketRW == null && retry <= 100)
            {
                retry++;
                await Task.Delay(100);
            }
            return SocketRW;
        }
        public void Dispose()
        {
            SocketRW?.Dispose();
            socketListener.Dispose();
            WfdDevice.Dispose();
        }
    }
}
