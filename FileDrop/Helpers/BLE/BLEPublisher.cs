using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth.Advertisement;
using Windows.Storage.Streams;

namespace FileDrop.Helpers.BLE
{
    public static class BLEPublisher
    {
        public static BluetoothLEAdvertisementPublisher publisher;
        private static string publishGUID;

        public static void StartPublish()
        {
            if (publisher == null)
                AddPublisherData();

            publisher.Start();
            SubscribeEvents();
        }

        public static void StopPublish()
        {
            if (publisher != null)
            {
                UnsubscribeEvents();
                publisher.Stop();
            }
        }

        private static void AddPublisherData()
        {
            publisher = new BluetoothLEAdvertisementPublisher();
            var manufacturerData = new BluetoothLEManufacturerData();
            manufacturerData.CompanyId = 0xFFFE;

            publishGUID = Guid.NewGuid().ToString("N").Substring(0, 8);

            var writer = new DataWriter();
            writer.WriteString($"FDJXT{publishGUID}");

            // Make sure that the buffer length can fit within an advertisement payload (~20 bytes). 
            // Otherwise you will get an exception.
            manufacturerData.Data = writer.DetachBuffer();

            // Add the manufacturer data to the advertisement publisher:
            publisher.Advertisement.ManufacturerData.Add(manufacturerData);
        }

        private static void SubscribeEvents()
        {
            publisher.StatusChanged += Publisher_StatusChanged;
        }

        private static void UnsubscribeEvents()
        {
            publisher.StatusChanged -= Publisher_StatusChanged;
        }

        private static void Publisher_StatusChanged(BluetoothLEAdvertisementPublisher sender, BluetoothLEAdvertisementPublisherStatusChangedEventArgs args)
        {

        }

        
    }
}
