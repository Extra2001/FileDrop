using FileDrop.Helpers.Dialog;
using FileDrop.Helpers.TransferHelper.Reviever;
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

namespace FileDrop.Helpers.WiFiDirect.Advertiser
{
    public class WiFiDirectAdvertiser
    {
        public static L2ConnectedDevice connectedDevice;
        private static WiFiDirectAdvertisementPublisher _publisher;
        private static WiFiDirectConnectionListener _listener;

        public static bool Started => _publisher != null && _publisher.Status == WiFiDirectAdvertisementPublisherStatus.Started;

        #region 对外暴露接口
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
        #endregion

        private static void OnConnectionRequested(WiFiDirectConnectionListener sender, WiFiDirectConnectionRequestedEventArgs connectionEventArgs)
        {
            ConnectedStatusManager.ReportProgress("有设备正在请求L2连接");
            WiFiDirectConnectionRequest connectionRequest = connectionEventArgs.GetConnectionRequest();

            App.mainWindow.DispatcherQueue.TryEnqueue(async () =>
            {
                ConnectedStatusManager
                    .ReportProgress($"设备\"{connectionRequest.DeviceInformation.Name}\"正在请求L2连接...");
                var ReadWrite = await HandleConnectionRequestAsync(connectionRequest);
                if (ReadWrite == null)
                {
                    connectionRequest.Dispose();
                }
                else
                {
                    var task = new RecieveTask();
                    await task.StartRecieve(ReadWrite);
                }
            });
        }
        private static async Task<SocketReaderWriter> HandleConnectionRequestAsync
           (WiFiDirectConnectionRequest connectionRequest)
        {
            bool isPaired = (connectionRequest.DeviceInformation.Pairing?.IsPaired == true) ||
                            (await IsAepPairedAsync(connectionRequest.DeviceInformation.Id));

            if (!isPaired && !_publisher.Advertisement.LegacySettings.IsEnabled)
            {
                if (!await ConnectHelper.RequestPairDeviceAsync(connectionRequest.DeviceInformation.Pairing))
                {
                    ConnectedStatusManager.ReportError(true, "未配对成功");
                    return null;
                }
            }

            if (connectedDevice != null)
            {
                try
                {
                    connectedDevice.Dispose();
                }
                catch { }
                connectedDevice = null;
            }

            ConnectedStatusManager.ReportProgress("正在接受L2连接");
            WiFiDirectDevice wfdDevice = null;
            try
            {
                wfdDevice = await WiFiDirectDevice.FromIdAsync(connectionRequest.DeviceInformation.Id);
            }
            catch (Exception ex)
            {
                ConnectedStatusManager.ReportError(true, "接受连接时出现错误" + ex.Message);
                return null;
            }

            // Register for the ConnectionStatusChanged event handler
            wfdDevice.ConnectionStatusChanged += OnConnectionStatusChanged;
            connectedDevice = new L2ConnectedDevice(wfdDevice);
            ConnectedStatusManager.ReportProgress("已接受L2连接，等待L4连接请求");
            var rw = await connectedDevice.EstablishSocket();
            if (rw != null)
            {
                ConnectedStatusManager.ReportProgress("成功建立L4连接");
            }
            else
            {
                ConnectedStatusManager.ReportError(true, "等待L4连接超时");
            }
            return rw;
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
