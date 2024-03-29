﻿using FileDrop.Helpers.Dialog;
using FileDrop.Helpers.TransferHelper.Reviever;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Devices.WiFiDirect;

namespace FileDrop.Helpers.WiFiDirect.Advertiser
{
    public class WiFiDirectAdvertiser
    {
        public static WiFiDirectDevice connectedDevice;
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
        }
        public static void CloseDevice()
        {
            RecieveTask.StopWaitForTransfer();
            if (connectedDevice != null)
            {
                try { connectedDevice.Dispose(); } catch { }
            }
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
                var success = await HandleConnectionRequestAsync(connectionRequest);
                if (!success)
                {
                    connectionRequest.Dispose();
                }
            });
        }
        private static async Task<bool> HandleConnectionRequestAsync(WiFiDirectConnectionRequest connectionRequest)
        {
            bool isPaired = (connectionRequest.DeviceInformation.Pairing?.IsPaired == true) ||
                            (await IsAepPairedAsync(connectionRequest.DeviceInformation.Id));

            if (!isPaired && !_publisher.Advertisement.LegacySettings.IsEnabled)
            {
                if (!await ConnectHelper.RequestPairDeviceAsync(connectionRequest.DeviceInformation.Pairing))
                {
                    ConnectedStatusManager.ReportError(true, "未配对成功");
                    return false;
                }
            }

            if (connectedDevice != null)
            {
                try { connectedDevice.Dispose(); }
                catch { }
                connectedDevice = null;
            }

            ConnectedStatusManager.ReportProgress("正在接受连接");
            WiFiDirectDevice wfdDevice = null;
            try
            {
                var para = new WiFiDirectConnectionParameters();
                para.PreferredPairingProcedure = WiFiDirectPairingProcedure.Invitation;
                para.PreferenceOrderedConfigurationMethods.Add(WiFiDirectConfigurationMethod.PushButton);
                wfdDevice = await WiFiDirectDevice.FromIdAsync(connectionRequest.DeviceInformation.Id, para);
            }
            catch (Exception ex)
            {
                ConnectedStatusManager.ReportError(true, "接受连接时出现错误：" + ex.Message);
                return false;
            }

            wfdDevice.ConnectionStatusChanged += OnConnectionStatusChanged;
            connectedDevice = wfdDevice;
            ConnectedStatusManager.ReportProgress("已建立L2连接，正在等待L4连接请求");
            RecieveTask.WaitForTransfer(wfdDevice.GetConnectionEndpointPairs()[0]);
            return true;
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
                CloseDevice();
            }
        }
    }
}
