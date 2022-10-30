using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth.Advertisement;
using Windows.Storage.Streams;

namespace FileDrop.Helpers.BLE
{
    public class BLEWatcher
    {
        public static BluetoothLEAdvertisementWatcher watcher;
        private static string publishGUID;

        public static void StartWatch()
        {
            if (watcher == null)
                AddWatcherData();
            watcher.Start();
            SubscribeEvents();
        }

        public static void StopWatch()
        {
            if (watcher != null)
            {
                UnsubscribeEvents();
                watcher.Stop();
            }
        }

        private static void AddWatcherData()
        {
            watcher = new BluetoothLEAdvertisementWatcher();
            var manufacturerData = new BluetoothLEManufacturerData();
            manufacturerData.CompanyId = 0xFFFE;

            publishGUID = Guid.NewGuid().ToString("N").Substring(0, 8);

            var writer = new DataWriter();
            writer.WriteString($"FDJXT{publishGUID}");

            // Make sure that the buffer length can fit within an advertisement payload (~20 bytes). 
            // Otherwise you will get an exception.
            manufacturerData.Data = writer.DetachBuffer();

            // Add the manufacturer data to the advertisement publisher:
            watcher.AdvertisementFilter.Advertisement.ManufacturerData.Add(manufacturerData);
            watcher.SignalStrengthFilter.InRangeThresholdInDBm = -70;

            // Set the out-of-range threshold to -75dBm (give some buffer). Used in conjunction with OutOfRangeTimeout
            // to determine when an advertisement is no longer considered "in-range"
            watcher.SignalStrengthFilter.OutOfRangeThresholdInDBm = -100;

            // Set the out-of-range timeout to be 2 seconds. Used in conjunction with OutOfRangeThresholdInDBm
            // to determine when an advertisement is no longer considered "in-range"
            watcher.SignalStrengthFilter.OutOfRangeTimeout = TimeSpan.FromMilliseconds(2000);
        }

        private static void SubscribeEvents()
        {
            // Attach a handler to process the received advertisement. 
            // The watcher cannot be started without a Received handler attached
            watcher.Received += OnAdvertisementReceived;
        }

        private static void UnsubscribeEvents()
        {
            watcher.Received -= OnAdvertisementReceived;
        }

        private static void OnAdvertisementReceived(BluetoothLEAdvertisementWatcher watcher, BluetoothLEAdvertisementReceivedEventArgs eventArgs)
        {
            // Check if there are any manufacturer-specific sections.
            // If there is, print the raw data of the first manufacturer section (if there are multiple).
            string publishGUID = "";
            ushort companyId = 0xFFFF;
            var manufacturerSections = eventArgs.Advertisement.ManufacturerData;
            if (manufacturerSections.Count > 0)
            {
                // Only print the first one of the list
                var manufacturerData = manufacturerSections[0];
                var data = new byte[manufacturerData.Data.Length];
                using (var reader = DataReader.FromBuffer(manufacturerData.Data))
                    reader.ReadBytes(data);
                publishGUID = BitConverter.ToString(data);
                companyId = manufacturerData.CompanyId;
            }
        }
    }
}
