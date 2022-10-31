using FileDrop.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Devices.Enumeration;
using Windows.Storage.Streams;
using Windows.UI.Core;

namespace FileDrop.Helpers.BLE
{
    public static class BLEClient
    {
        private static List<DeviceInformation> Devices = new List<DeviceInformation>();
        public static ObservableCollection<DeviceContent> deviceContents
            = new ObservableCollection<DeviceContent>();

        private static DeviceWatcher deviceWatcher;

        #region Events
        public delegate void _EnumerateComplete();
        public static event _EnumerateComplete ScanComplete;
        #endregion

        #region Error Codes
        private static int E_BLUETOOTH_ATT_WRITE_NOT_PERMITTED = unchecked((int)0x80650003);
        private static int E_BLUETOOTH_ATT_INVALID_PDU = unchecked((int)0x80650004);
        private static int E_ACCESSDENIED = unchecked((int)0x80070005);
        private static int E_DEVICE_NOT_AVAILABLE = unchecked((int)0x800710df);
        #endregion

        public static void StartBleDeviceWatcher()
        {
            if (deviceWatcher == null)
            {
                string[] requestedProperties = { "System.Devices.Aep.DeviceAddress", "System.Devices.Aep.IsConnected", "System.Devices.Aep.Bluetooth.Le.IsConnectable", "System.Devices.Aep.IsPresent" };

                string aqsAllBluetoothLEDevices = "(System.Devices.Aep.ProtocolId:=\"{bb7bb05e-5972-42b5-94fc-76eaa7084d49}\")";

                deviceWatcher =
                        DeviceInformation.CreateWatcher(
                            aqsAllBluetoothLEDevices,
                            requestedProperties,
                            DeviceInformationKind.AssociationEndpoint);

                deviceWatcher.Added += DeviceWatcher_Added;
                deviceWatcher.Updated += DeviceWatcher_Updated;
                deviceWatcher.Removed += DeviceWatcher_Removed;
                deviceWatcher.EnumerationCompleted += DeviceWatcher_EnumerationCompleted;
                deviceWatcher.Stopped += DeviceWatcher_Stopped;
            }

            Devices.Clear();
            deviceContents.Clear();

            deviceWatcher.Start();
        }
        public static void StopBleDeviceWatcher()
        {
            if (deviceWatcher != null)
            {
                // Unregister the event handlers.
                deviceWatcher.Added -= DeviceWatcher_Added;
                deviceWatcher.Updated -= DeviceWatcher_Updated;
                deviceWatcher.Removed -= DeviceWatcher_Removed;
                deviceWatcher.EnumerationCompleted -= DeviceWatcher_EnumerationCompleted;
                deviceWatcher.Stopped -= DeviceWatcher_Stopped;

                // Stop the watcher.
                deviceWatcher.Stop();
                deviceWatcher = null;
            }
        }
        private static void DeviceWatcher_Added(DeviceWatcher sender, DeviceInformation deviceInfo)
        {
            Devices.Add(deviceInfo);
            ConnectDevice(deviceInfo);
        }
        private static void DeviceWatcher_Updated(DeviceWatcher sender, DeviceInformationUpdate deviceInfoUpdate)
        {
            var id = deviceInfoUpdate.Id;
            var d = Devices.Where(x => x.Id == id).FirstOrDefault();
            if (d == null) return;
            d.Update(deviceInfoUpdate);
            ConnectDevice(d);
        }
        private static void DeviceWatcher_Removed(DeviceWatcher sender, DeviceInformationUpdate deviceInfoUpdate)
        {
            var id = deviceInfoUpdate.Id;
            var dc = deviceContents.Where(x => x.deviceInfo.Id == id).FirstOrDefault();
            var d = Devices.Where(x => x.Id == id).FirstOrDefault();
            if (dc != null)
                deviceContents.Remove(dc);
            if (d != null)
                Devices.Remove(d);
        }
        private static void DeviceWatcher_EnumerationCompleted
            (DeviceWatcher sender, object e)
        {
            ScanComplete?.Invoke();
        }
        private static void DeviceWatcher_Stopped(DeviceWatcher sender, object e)
        {
            ScanComplete?.Invoke();
        }
        private static async void ConnectDevice(DeviceInformation deviceInfo)
        {
            try
            {
                using (BluetoothLEDevice bluetoothLeDevice = await BluetoothLEDevice.FromIdAsync(deviceInfo.Id))
                {
                    await IsSupported(deviceInfo, bluetoothLeDevice);
                }
            }
            catch (ArgumentException ex)
            {
                throw new Exception("请重新安装蓝牙驱动。", ex);
            }
            catch (Exception ex) when (ex.HResult == E_DEVICE_NOT_AVAILABLE)
            {
                throw new Exception("蓝牙未开启。", ex);
            }
        }
        private static async Task<bool> IsSupported(DeviceInformation deviceInfo, BluetoothLEDevice device)
        {
            GattDeviceServicesResult result = await device.GetGattServicesAsync();
            if (result.Status == GattCommunicationStatus.Success)
            {
                var services = result.Services;
                var service = services.Where(x => x.Uuid == ServiceDefinition.ServiceUUID).FirstOrDefault();
                if (service == null) return UpdateDeviceContents(deviceInfo, null);
                var accessStatus = await service.RequestAccessAsync();
                if (accessStatus == DeviceAccessStatus.Allowed)
                {
                    var cha = await service.GetCharacteristicsAsync(BluetoothCacheMode.Uncached);
                    if (cha.Status == GattCommunicationStatus.Success)
                    {
                        var characteristics = cha.Characteristics;
                        var appInfoCha = characteristics
                            .Where(x => x.Uuid == ServiceDefinition.AppInfoCharacteristic).FirstOrDefault();
                        if (appInfoCha == null) return UpdateDeviceContents(deviceInfo, null);
                        var appInfoRes = await appInfoCha.ReadValueAsync(BluetoothCacheMode.Uncached);
                        if (appInfoRes.Status == GattCommunicationStatus.Success)
                        {
                            string appInfo = appInfoRes.Value.Parse<AppInfo>().DeviceName;
                            return UpdateDeviceContents(deviceInfo, appInfo);
                        }
                    }
                }
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
