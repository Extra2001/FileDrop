using FileDrop.Helpers.Dialog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.WiFiDirect;
using Windows.Networking;
using Windows.Networking.Sockets;

namespace FileDrop.Helpers.WiFiDirect
{
    public class ConnectedDevice : IDisposable
    {
        public WiFiDirectDevice WfdDevice { get; }
        public StreamSocketListener socketListener { get; }
        private HostName remoteHostName { get; }
        private SocketReaderWriter SocketRW { get; set; }
        private bool fromConnector { get; set; }

        public delegate void _RecievedSocketConnection(ConnectedDevice device, SocketReaderWriter socket);
        public event _RecievedSocketConnection RecievedSocketConnection;

        public ConnectedDevice(WiFiDirectDevice wfdDevice, bool fromConnector)
        {
            WfdDevice = wfdDevice;
            IReadOnlyList<EndpointPair> endpointPairs = wfdDevice.GetConnectionEndpointPairs();
            remoteHostName = endpointPairs[0].RemoteHostName;
            socketListener = new StreamSocketListener();
            socketListener.ConnectionReceived += SocketListener_ConnectionReceived;
            if (!fromConnector)
                _ = socketListener.BindEndpointAsync(remoteHostName, ConnectDefinition.strServerPort);
            this.fromConnector = fromConnector;
        }

        private void SocketListener_ConnectionReceived(StreamSocketListener sender, StreamSocketListenerConnectionReceivedEventArgs args)
        {
            SocketRW = new SocketReaderWriter(args.Socket);
            RecievedSocketConnection?.Invoke(this, SocketRW);
        }

        public async Task<SocketReaderWriter> EstablishSocket()
        {
            if (SocketRW != null || !fromConnector)
                return SocketRW;
            ModelDialog.ShowWaiting("请稍后", "正在建立L4连接...");
            StreamSocket clientSocket = new StreamSocket();
            try
            {
                await clientSocket.ConnectAsync(remoteHostName, ConnectDefinition.strServerPort);
            }
            catch (Exception ex)
            {
                _ = ModelDialog.ShowDialog("提示", "出现错误：" + ex.Message);
                throw new Exception("出现错误：" + ex.Message);
            }
            SocketRW = new SocketReaderWriter(clientSocket);
            return SocketRW;
        }
        public void CloseSocket()
        {
            if (SocketRW == null)
                return;
            try { SocketRW.Dispose(); }
            catch { }
            SocketRW = null;
        }

        public void Dispose()
        {
            SocketRW?.Dispose();
            socketListener.Dispose();
            WfdDevice.Dispose();
        }
    }
}
