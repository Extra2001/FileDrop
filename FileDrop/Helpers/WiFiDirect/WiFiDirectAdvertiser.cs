using FileDrop.Helpers.Dialog;
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
        public static ConnectedDevice connectedDevice;
        private static WiFiDirectAdvertisementPublisher _publisher;
        private static WiFiDirectConnectionListener _listener;

        public static bool Started => _publisher != null && _publisher.Status == WiFiDirectAdvertisementPublisherStatus.Started;

        public static void StartAdvertisement()
        {
            if (!Started)
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
                    _ = ModelDialog.ShowDialog("未开启WiFi", "您将无法正常使用功能", "确定");
                    return;
                }
                _publisher.Advertisement.ListenStateDiscoverability
                    = WiFiDirectAdvertisementListenStateDiscoverability.Normal;
                foreach (var item in ConnectDefinition.GetInformationElements())
                    _publisher.Advertisement.InformationElements.Add(item);
            }
            _publisher.Start();
            if (_publisher.Status != WiFiDirectAdvertisementPublisherStatus.Started)
            {
                _ = ModelDialog.ShowDialog("开启广播失败", "您将无法正常使用功能", "确定");
            }
        }

        public static void StopAdvertisement()
        {
            if (Started)
            {
                _publisher.Stop();
                _publisher.StatusChanged -= OnStatusChanged;
            }
            _listener.ConnectionRequested -= OnConnectionRequested;
            connectedDevice?.Dispose();
            connectedDevice = null;
        }

        private static async Task<bool> HandleConnectionRequestAsync
            (WiFiDirectConnectionRequest connectionRequest)
        {
            //bool isPaired = (connectionRequest.DeviceInformation.Pairing?.IsPaired == true) ||
            //                (await IsAepPairedAsync(connectionRequest.DeviceInformation.Id));

            //// Pair device if not already paired and not using legacy settings
            //if (!isPaired && !_publisher.Advertisement.LegacySettings.IsEnabled)
            //{
            //    if (!await ConnectHelper.RequestPairDeviceAsync(connectionRequest.DeviceInformation.Pairing))
            //    {
            //        return false;
            //    }
            //}

            if(connectedDevice!=null)
            {
                try
                {
                    connectedDevice.Dispose();
                }
                catch { }
                connectedDevice = null;
            }

            WiFiDirectDevice wfdDevice = null;
            try
            {
                wfdDevice = await WiFiDirectDevice.FromIdAsync(connectionRequest.DeviceInformation.Id);
            }
            catch (Exception ex)
            {
                _ = ModelDialog.ShowDialog("错误", $"连接失败{ex.Message}");
                return false;
            }

            // Register for the ConnectionStatusChanged event handler
            wfdDevice.ConnectionStatusChanged += OnConnectionStatusChanged;
            connectedDevice = new ConnectedDevice(wfdDevice);
            connectedDevice.RecievedSocketConnection += ConnectedDevice_RecievedSocketConnection;

            return true;
        }

        private static void ConnectedDevice_RecievedSocketConnection(ConnectedDevice device, SocketReaderWriter socket)
        {
            ModelDialog.ShowWaiting("请稍后", $"已建立连接，等待对方发送传输请求...");
            socket.StartRead(SocketRead.RecieveRead);
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
            ModelDialog.ShowWaiting("请稍后", $"设备正在请求连接...");
            WiFiDirectConnectionRequest connectionRequest = connectionEventArgs.GetConnectionRequest();
            ModelDialog.ShowWaiting("请稍后", $"设备\"{connectionRequest.DeviceInformation.Name}\"正在请求连接...");
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
                try
                {
                    connectedDevice.Dispose();
                }
                catch { }
                connectedDevice = null;
            }
        }
    }
}
