using FileDrop.Models.BluetoothLE;
using InTheHand.Bluetooth;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Devices.Enumeration;
using Windows.UI.Core;

namespace FileDrop.Helpers.BLE
{
    public static class BLEClient
    {
        private static List<DeviceInformation> Devices = new List<DeviceInformation>();
        private static List<DeviceContent> deviceContents = new List<DeviceContent>();

        private static DeviceWatcher deviceWatcher;

        public static void StartBleDeviceWatcher()
        {
            // Additional properties we would like about the device.
            // Property strings are documented here https://msdn.microsoft.com/en-us/library/windows/desktop/ff521659(v=vs.85).aspx
            string[] requestedProperties = { "System.Devices.Aep.DeviceAddress", "System.Devices.Aep.IsConnected", "System.Devices.Aep.Bluetooth.Le.IsConnectable", "System.Devices.Aep.IsPresent" };

            // BT_Code: Example showing paired and non-paired in a single query.
            string aqsAllBluetoothLEDevices = "(System.Devices.Aep.ProtocolId:=\"{bb7bb05e-5972-42b5-94fc-76eaa7084d49}\")";

            deviceWatcher =
                    DeviceInformation.CreateWatcher(
                        aqsAllBluetoothLEDevices,
                        requestedProperties,
                        DeviceInformationKind.AssociationEndpoint);

            // Register event handlers before starting the watcher.
            deviceWatcher.Added += DeviceWatcher_Added;
            deviceWatcher.Updated += DeviceWatcher_Updated;
            deviceWatcher.Removed += DeviceWatcher_Removed;
            deviceWatcher.EnumerationCompleted += DeviceWatcher_EnumerationCompleted;
            deviceWatcher.Stopped += DeviceWatcher_Stopped;

            // Start over with an empty collection.
            Devices.Clear();

            deviceWatcher.Start();
        }

        /// <summary>
        /// Stops watching for all nearby Bluetooth devices.
        /// </summary>
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

        private static async void DeviceWatcher_Added(DeviceWatcher sender, DeviceInformation deviceInfo)
        {
            Devices.Add(deviceInfo);
            ConnectDevice(deviceInfo);
        }

        private static async void DeviceWatcher_Updated(DeviceWatcher sender, DeviceInformationUpdate deviceInfoUpdate)
        {
            var id = deviceInfoUpdate.Id;
            var dc = deviceContents.Where(x => x.deviceInfo.Id == id).FirstOrDefault();
            if (dc == null) return;
            dc.deviceInfo.Update(deviceInfoUpdate);
            var deviceInfo = dc.deviceInfo;
            Devices.Add(deviceInfo);
            ConnectDevice(deviceInfo);
        }

        private static async void DeviceWatcher_Removed(DeviceWatcher sender, DeviceInformationUpdate deviceInfoUpdate)
        {
            var id = deviceInfoUpdate.Id;
            var dc = deviceContents.Where(x => x.deviceInfo.Id == id).FirstOrDefault();
            if (dc == null) return;
            deviceContents.Remove(dc);
        }

        private static async void DeviceWatcher_EnumerationCompleted(DeviceWatcher sender, object e)
        {

        }

        private static async void DeviceWatcher_Stopped(DeviceWatcher sender, object e)
        {

        }

        private static async void ConnectDevice(DeviceInformation deviceInfo)
        {
            try
            {
                using (BluetoothLEDevice bluetoothLeDevice = await BluetoothLEDevice.FromIdAsync(deviceInfo.Id))
                {
                    GattDeviceServicesResult result = await bluetoothLeDevice.GetGattServicesAsync();
                    if (result.Status == GattCommunicationStatus.Success)
                    {
                        var services = result.Services;
                        var service = services.Where(x => x.Uuid == ServiceDefinition.ServiceUUID).FirstOrDefault();
                        if (service == null) return;
                        deviceContents.Add(new DeviceContent()
                        {
                            deviceInfo = deviceInfo,
                            service = service,
                            applySendCharacteristic = service
                                .GetCharacteristics(ServiceDefinition.ApplySendCharacteristic).FirstOrDefault(),
                            appInfoCharacteristic = service
                                .GetCharacteristics(ServiceDefinition.AppInfoCharacteristic).FirstOrDefault(),
                            permitCharacteristic = service
                                .GetCharacteristics(ServiceDefinition.PermitCharacteristic).FirstOrDefault()
                        });
                    }
                }
            }
            catch { }
        }
    }
}
