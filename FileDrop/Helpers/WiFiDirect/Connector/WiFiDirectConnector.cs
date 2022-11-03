using FileDrop.Helpers.BLE;
using FileDrop.Models;
using Microsoft.UI.Xaml;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Devices.Bluetooth;
using Windows.Devices.Enumeration;
using Windows.Devices.WiFiDirect;
using Windows.Networking.Sockets;
using Windows.Security.Cryptography;
using Windows.Storage.Streams;
using Windows.UI.Core;
using Windows.UI.Popups;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Networking;
using FileDrop.Helpers.Dialog;
using FileDrop.Helpers.WiFiDirect.Connector;

namespace FileDrop.Helpers.WiFiDirect.Connector
{
    public class WiFiDirectConnector
    {
        private static WiFiDirectAdvertisementPublisher _publisher = new WiFiDirectAdvertisementPublisher();
        private static DeviceWatcher _deviceWatcher = null;
        public static ObservableCollection<DiscoveredDevice> discoveredDevices { get; }
            = new ObservableCollection<DiscoveredDevice>();
        public static L2ConnectDevice connectedDevice;
        public static ObservableCollection<DeviceContent> deviceContents { get; }
            = new ObservableCollection<DeviceContent>();

        #region Events
        public delegate void _EnumerateComplete();
        public static event _EnumerateComplete ScanComplete;
        public delegate void _EnumerateStarted();
        public static event _EnumerateStarted ScanStart;
        #endregion

        #region 对外暴露接口
        public static void StartWatcher()
        {
            // 启动publisher
            StopWatcher();
            if (_publisher.Status != WiFiDirectAdvertisementPublisherStatus.Started)
                _publisher.Start();
            if (_publisher.Status != WiFiDirectAdvertisementPublisherStatus.Started)
                return;

            discoveredDevices.Clear();
            deviceContents.Clear();

            string deviceSelector = WiFiDirectDevice.GetDeviceSelector(WiFiDirectDeviceSelectorType.AssociationEndpoint);
            _deviceWatcher = DeviceInformation.CreateWatcher(deviceSelector, new string[] { "System.Devices.WiFiDirect.InformationElements" });

            _deviceWatcher.Added += OnDeviceAdded;
            _deviceWatcher.Removed += OnDeviceRemoved;
            _deviceWatcher.Updated += OnDeviceUpdated;
            _deviceWatcher.EnumerationCompleted += OnEnumerationCompleted;
            _deviceWatcher.Stopped += OnStopped;

            _deviceWatcher.Start();
            ScanStart?.Invoke();
        }
        public static void StopWatcher()
        {
            if (_publisher.Status == WiFiDirectAdvertisementPublisherStatus.Started)
                _publisher.Stop();
            if (_deviceWatcher != null)
            {
                try
                {
                    _deviceWatcher.Added -= OnDeviceAdded;
                    _deviceWatcher.Removed -= OnDeviceRemoved;
                    _deviceWatcher.Updated -= OnDeviceUpdated;
                    _deviceWatcher.EnumerationCompleted -= OnEnumerationCompleted;
                    _deviceWatcher.Stopped -= OnStopped;
                    _deviceWatcher.Stop();
                }
                catch { }
            }
            _deviceWatcher = null;
        }
        #endregion

        #region DeviceWatcherEvents
        private static void OnDeviceAdded(DeviceWatcher deviceWatcher, DeviceInformation deviceInfo)
        {
            discoveredDevices.Add(new DiscoveredDevice(deviceInfo));
            IsSupported(deviceInfo);
        }
        private static void OnDeviceRemoved(DeviceWatcher deviceWatcher, DeviceInformationUpdate deviceInfoUpdate)
        {
            foreach (DiscoveredDevice discoveredDevice in discoveredDevices)
            {
                if (discoveredDevice.DeviceInfo.Id == deviceInfoUpdate.Id)
                {
                    discoveredDevices.Remove(discoveredDevice);
                    UpdateDeviceContents(discoveredDevice.DeviceInfo, null);
                    break;
                }
            }
        }
        private static void OnDeviceUpdated(DeviceWatcher deviceWatcher, DeviceInformationUpdate deviceInfoUpdate)
        {
            foreach (DiscoveredDevice discoveredDevice in discoveredDevices)
            {
                if (discoveredDevice.DeviceInfo.Id == deviceInfoUpdate.Id)
                {
                    discoveredDevice.UpdateDeviceInfo(deviceInfoUpdate);
                    IsSupported(discoveredDevice.DeviceInfo);
                    break;
                }
            }
        }
        private static void OnEnumerationCompleted(DeviceWatcher deviceWatcher, object o)
        {
            ScanComplete?.Invoke();
        }
        private static void OnStopped(DeviceWatcher deviceWatcher, object o)
        {
            ScanComplete?.Invoke();
        }
        #endregion

        #region 设备筛选
        private static bool IsSupported(DeviceInformation deviceInfo)
        {
            IList<WiFiDirectInformationElement> informationElements = null;
            try
            {
                informationElements = WiFiDirectInformationElement.CreateFromDeviceInformation(deviceInfo);
            }
            catch (Exception) { }

            if (informationElements != null)
            {
                string deviceName = null;
                foreach (WiFiDirectInformationElement informationElement in informationElements)
                {
                    string value = string.Empty;
                    byte[] bOui = informationElement.Oui.ToArray();

                    if (bOui.SequenceEqual(ConnectDefinition.AppOui))
                    {
                        if (informationElement.OuiType == ConnectDefinition.AppInfoOuiType)
                        {
                            try
                            {
                                var str = informationElement.Value.Parse();
                                if (str != "FileDrop v1.0.0")
                                    return UpdateDeviceContents(deviceInfo, null);
                            }
                            catch (Exception)
                            {
                                return UpdateDeviceContents(deviceInfo, null);
                            }
                        }
                        else if (informationElement.OuiType == ConnectDefinition.DeviceNameOuiType)
                        {
                            try
                            {
                                deviceName = informationElement.Value.Parse();
                            }
                            catch (Exception)
                            {
                                return UpdateDeviceContents(deviceInfo, null);
                            }
                        }
                    }
                }
                return UpdateDeviceContents(deviceInfo, deviceName);
            }
            return UpdateDeviceContents(deviceInfo, null);
        }
        private static bool UpdateDeviceContents(DeviceInformation deviceInfo, string deviceName)
        {
            var dc = deviceContents.Where(x => x.deviceInfo.Id == deviceInfo.Id).FirstOrDefault();
            if (dc == null && string.IsNullOrEmpty(deviceName)) return false;

            if (dc == null)
            {
                int id = 1;
                if (deviceContents.Any())
                    id = deviceContents.Select(x => x.Id).Max() + 1;
                deviceContents.Add(new DeviceContent()
                {
                    Id = id,
                    deviceName = deviceName,
                    deviceInfo = deviceInfo
                });
            }
            else if (deviceName == null)
            {
                deviceContents.Remove(dc);
                return false;
            }
            else
            {
                dc.deviceName = deviceName;
            }

            return true;
        }
        #endregion

        public static void ConnectDevice(DeviceInformation deviceInfo, Action<SocketReaderWriter> callback)
        {
            StopWatcher();
            App.mainWindow.DispatcherQueue.TryEnqueue(async () =>
            {
                ConnectStatusManager.ReportProgress("开始发起连接");

                if (connectedDevice != null)
                {
                    try { connectedDevice.Dispose(); } catch { }
                }
                if (!deviceInfo.Pairing.IsPaired)
                {
                    if (!await ConnectHelper.RequestPairDeviceAsync(deviceInfo.Pairing))
                    {
                        ConnectStatusManager.ReportError(true, "尝试配对失败");
                        callback.Invoke(null);
                        return;
                    }
                }

                int retry = 0;
                WiFiDirectDevice wfdDevice = null;
                tryConnect:
                try
                {
                    var para = new WiFiDirectConnectionParameters();
                    para.PreferredPairingProcedure = WiFiDirectPairingProcedure.Invitation;
                    para.PreferenceOrderedConfigurationMethods.Add(WiFiDirectConfigurationMethod.PushButton);
                    wfdDevice = await WiFiDirectDevice.FromIdAsync(deviceInfo.Id, para);
                }
                catch (TaskCanceledException)
                {
                    ConnectStatusManager.ReportError(true, "发送已被取消");
                    callback.Invoke(null);
                    return;
                }
                catch (Exception ex) when (ex.HResult == unchecked((int)0x8007001F))
                {
                    if (retry < 50)
                    {
                        retry++;
                        goto tryConnect;
                    }
                    else
                    {
                        ConnectStatusManager.ReportError(true, "创建L2连接时发生异常：" + ex.Message);
                        callback.Invoke(null);
                        return;
                    }
                }
                catch (Exception ex)
                {
                    ConnectStatusManager.ReportError(true, "创建L2连接时发生异常：" + ex.Message);
                    callback.Invoke(null);
                    return;
                }

                wfdDevice.ConnectionStatusChanged += OnConnectionStatusChanged;

                connectedDevice = new L2ConnectDevice(wfdDevice);

                ConnectStatusManager.ReportProgress("L2连接建立成功，正在建立L4连接");

                var RW = await connectedDevice.EstablishSocket();
                if (RW != null)
                {
                    ConnectStatusManager.ReportProgress("L4连接建立成功");
                }
                callback.Invoke(RW);
            });
        }
        private static void OnConnectionStatusChanged(WiFiDirectDevice sender, object arg)
        {

        }
    }
}
