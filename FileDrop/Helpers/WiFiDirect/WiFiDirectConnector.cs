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

namespace FileDrop.Helpers.WiFiDirect
{
    public class WiFiDirectConnector
    {
        private static DeviceWatcher _deviceWatcher = null;

        public static ObservableCollection<DiscoveredDevice> DiscoveredDevices { get; }
            = new ObservableCollection<DiscoveredDevice>();
        public static ObservableCollection<ConnectedDevice> ConnectedDevices { get; }
            = new ObservableCollection<ConnectedDevice>();
        public static ObservableCollection<DeviceContent> deviceContents { get; }
            = new ObservableCollection<DeviceContent>();

        #region Events
        public delegate void _EnumerateComplete();
        public static event _EnumerateComplete ScanComplete;
        #endregion

        public static void StartWatcher()
        {
            if (!WiFiDirectAdvertiser.Started)
                WiFiDirectAdvertiser.StartAdvertisement();
            if (!WiFiDirectAdvertiser.Started)
                return;
            DiscoveredDevices.Clear();

            string deviceSelector = WiFiDirectDevice.GetDeviceSelector(WiFiDirectDeviceSelectorType.AssociationEndpoint);

            _deviceWatcher = DeviceInformation.CreateWatcher(deviceSelector, new string[] { "System.Devices.WiFiDirect.InformationElements" });

            _deviceWatcher.Added += OnDeviceAdded;
            _deviceWatcher.Removed += OnDeviceRemoved;
            _deviceWatcher.Updated += OnDeviceUpdated;
            _deviceWatcher.EnumerationCompleted += OnEnumerationCompleted;
            _deviceWatcher.Stopped += OnStopped;

            _deviceWatcher.Start();
        }

        public static void StopWatcher()
        {
            _deviceWatcher.Added -= OnDeviceAdded;
            _deviceWatcher.Removed -= OnDeviceRemoved;
            _deviceWatcher.Updated -= OnDeviceUpdated;
            _deviceWatcher.EnumerationCompleted -= OnEnumerationCompleted;
            _deviceWatcher.Stopped -= OnStopped;
            _deviceWatcher.Stop();
            _deviceWatcher = null;
        }

        #region DeviceWatcherEvents
        private static void OnDeviceAdded(DeviceWatcher deviceWatcher, DeviceInformation deviceInfo)
        {
            DiscoveredDevices.Add(new DiscoveredDevice(deviceInfo));
            IsSupported(deviceInfo);
        }
        private static void OnDeviceRemoved(DeviceWatcher deviceWatcher, DeviceInformationUpdate deviceInfoUpdate)
        {
            foreach (DiscoveredDevice discoveredDevice in DiscoveredDevices)
            {
                if (discoveredDevice.DeviceInfo.Id == deviceInfoUpdate.Id)
                {
                    DiscoveredDevices.Remove(discoveredDevice);
                    UpdateDeviceContents(discoveredDevice.DeviceInfo, null);
                    break;
                }
            }
        }
        private static void OnDeviceUpdated(DeviceWatcher deviceWatcher, DeviceInformationUpdate deviceInfoUpdate)
        {
            foreach (DiscoveredDevice discoveredDevice in DiscoveredDevices)
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

        private static bool IsSupported(DeviceInformation deviceInfo)
        {
            IList<WiFiDirectInformationElement> informationElements = null;
            try
            {
                informationElements = WiFiDirectInformationElement.CreateFromDeviceInformation(deviceInfo);
            }
            catch (Exception)
            {
                
            }

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
                        else if(informationElement.OuiType == ConnectDefinition.DeviceNameOuiType)
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
    }
}
