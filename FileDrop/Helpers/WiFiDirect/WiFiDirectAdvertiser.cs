using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Devices.WiFiDirect;
using Windows.Networking.Sockets;
using Windows.Security.Cryptography;
using Windows.Storage.Streams;

namespace FileDrop.Helpers.WiFiDirect
{
    public class WiFiDirectAdvertiser
    {
        private static ObservableCollection<ConnectedDevice> ConnectedDevices
            = new ObservableCollection<ConnectedDevice>();
        private static WiFiDirectAdvertisementPublisher _publisher;
        private static WiFiDirectConnectionListener _listener;
        private static ConcurrentDictionary<StreamSocketListener, WiFiDirectDevice> _pendingConnections
            = new ConcurrentDictionary<StreamSocketListener, WiFiDirectDevice>();

        public static bool Started => _publisher != null && _publisher.Status == WiFiDirectAdvertisementPublisherStatus.Started;

        public static void StartAdvertisement()
        {
            _publisher = new WiFiDirectAdvertisementPublisher();
            _publisher.StatusChanged += OnStatusChanged;

            _listener = new WiFiDirectConnectionListener();

            try
            {
                // This can raise an exception if the machine does not support WiFi. Sorry.
                _listener.ConnectionRequested += OnConnectionRequested;
            }
            catch (Exception)
            {
                // rootPage.NotifyUser($"Error preparing Advertisement: {ex}", NotifyType.ErrorMessage);
                return;
            }

            _publisher.Advertisement.ListenStateDiscoverability
                = WiFiDirectAdvertisementListenStateDiscoverability.Normal;

            foreach (var item in ConnectDefinition.GetInformationElements())
                _publisher.Advertisement.InformationElements.Add(item);

            _publisher.Start();

            if (_publisher.Status != WiFiDirectAdvertisementPublisherStatus.Started)
            {

            }
        }

        public static void StopAdvertisement()
        {
            _publisher.Stop();
            _publisher.StatusChanged -= OnStatusChanged;
            _listener.ConnectionRequested -= OnConnectionRequested;
        }

        private static async Task<bool> HandleConnectionRequestAsync
            (WiFiDirectConnectionRequest connectionRequest)
        {
            bool isPaired = (connectionRequest.DeviceInformation.Pairing?.IsPaired == true) ||
                            (await IsAepPairedAsync(connectionRequest.DeviceInformation.Id));

            // Pair device if not already paired and not using legacy settings
            if (!isPaired && !_publisher.Advertisement.LegacySettings.IsEnabled)
            {
                if (!await ConnectHelper.RequestPairDeviceAsync(connectionRequest.DeviceInformation.Pairing))
                {
                    return false;
                }
            }
            return true;
        }

        public static async Task<bool> ConnectDevice(WiFiDirectConnectionRequest connectionRequest)
        {
            WiFiDirectDevice wfdDevice = null;
            try
            {
                // IMPORTANT: FromIdAsync needs to be called from the UI thread
                wfdDevice = await WiFiDirectDevice.FromIdAsync(connectionRequest.DeviceInformation.Id);
            }
            catch (Exception ex)
            {
                return false;
            }

            // Register for the ConnectionStatusChanged event handler
            wfdDevice.ConnectionStatusChanged += OnConnectionStatusChanged;

            var listenerSocket = new StreamSocketListener();

            // Save this (listenerSocket, wfdDevice) pair so we can hook it up when the socket connection is made.
            _pendingConnections[listenerSocket] = wfdDevice;

            var EndpointPairs = wfdDevice.GetConnectionEndpointPairs();

            listenerSocket.ConnectionReceived += OnSocketConnectionReceived;
            try
            {
                await listenerSocket.BindEndpointAsync(EndpointPairs[0].LocalHostName, ConnectDefinition.strServerPort);
            }
            catch (Exception ex)
            {
                return false;
            }
            return true;
        }

        private static async void OnSocketConnectionReceived(StreamSocketListener sender, StreamSocketListenerConnectionReceivedEventArgs args)
        {
            StreamSocket serverSocket = args.Socket;

            // Look up the WiFiDirectDevice associated with this StreamSocketListener.
            WiFiDirectDevice wfdDevice;
            if (!_pendingConnections.TryRemove(sender, out wfdDevice))
            {
                serverSocket.Dispose();
                return;
            }

            SocketReaderWriter socketRW = new SocketReaderWriter(serverSocket);

            // The first message sent is the name of the connection.
            string message = await socketRW.ReadMessageAsync();

            // Add this connection to the list of active connections.
            ConnectedDevices.Add(new ConnectedDevice(message ?? "(unnamed)", wfdDevice, socketRW));

            while (message != null)
            {
                message = await socketRW.ReadMessageAsync();
            }
        }


        private static async Task<bool> IsAepPairedAsync(string deviceId)
        {
            List<string> additionalProperties = new List<string>();
            additionalProperties.Add("System.Devices.Aep.DeviceAddress");
            DeviceInformation devInfo = null;

            try
            {
                devInfo = await DeviceInformation.CreateFromIdAsync(deviceId, additionalProperties);
            }
            catch (Exception)
            {

            }

            if (devInfo == null)
            {
                return false;
            }

            var deviceSelector = $"System.Devices.Aep.DeviceAddress:=\"{devInfo.Properties["System.Devices.Aep.DeviceAddress"]}\"";
            DeviceInformationCollection pairedDeviceCollection = await DeviceInformation.FindAllAsync(deviceSelector, null, DeviceInformationKind.Device);
            return pairedDeviceCollection.Count > 0;
        }

        private static async void OnConnectionRequested(WiFiDirectConnectionListener sender, WiFiDirectConnectionRequestedEventArgs connectionEventArgs)
        {
            WiFiDirectConnectionRequest connectionRequest = connectionEventArgs.GetConnectionRequest();
            bool success = await HandleConnectionRequestAsync(connectionRequest);

            if (!success)
            {
                connectionRequest.Dispose();
            }
        }

        private static void OnStatusChanged(WiFiDirectAdvertisementPublisher sender, WiFiDirectAdvertisementPublisherStatusChangedEventArgs statusEventArgs)
        {
            if (statusEventArgs.Status == WiFiDirectAdvertisementPublisherStatus.Started)
            {

            }
        }
        private static void OnConnectionStatusChanged(WiFiDirectDevice sender, object arg)
        {
            if (sender.ConnectionStatus == WiFiDirectConnectionStatus.Disconnected)
            {
                // TODO: Should we remove this connection from the list?
                // (Yes, probably.)
            }
        }
    }
}
