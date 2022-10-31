using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Devices.Bluetooth;
using Windows.Storage.Streams;

namespace FileDrop.Helpers.BLE
{
    public static class BLEServer
    {
        private static GattServiceProvider serviceProvider;
        private static GattLocalCharacteristic applySendCharacteristic;
        private static GattLocalCharacteristic permitCharacteristic;
        private static GattLocalCharacteristic appInfoCharacteristic;

        public static bool Started =>
            serviceProvider.AdvertisementStatus != GattServiceProviderAdvertisementStatus.Stopped ||
            serviceProvider.AdvertisementStatus != GattServiceProviderAdvertisementStatus.Aborted;

        public static void StartServer()
        {
            GattServiceProviderAdvertisingParameters advParameters = new GattServiceProviderAdvertisingParameters
            {
                IsConnectable = true,
                IsDiscoverable = true
            };
            serviceProvider.StartAdvertising(advParameters);
        }
        public static void StopServer()
        {
            serviceProvider.StopAdvertising();
        }
        public static async Task<bool> InitServiceAsync()
        {
            var s1 = await CreateService();
            var c1 = await CreateAppInfoCharacteristic();
            var c2 = await CreatePermitCharacteristic();
            var c3 = await CreateSendApplyCharacteristic();
            return s1 && c1 && c2 && c3;
        }
        private static async Task<bool> CreateService()
        {
            // BT_Code: Initialize and starting a custom GATT Service using GattServiceProvider.
            GattServiceProviderResult serviceResult =
                await GattServiceProvider.CreateAsync(ServiceDefinition.ServiceUUID);
            if (serviceResult.Error == BluetoothError.Success)
            {
                serviceProvider = serviceResult.ServiceProvider;
            }
            else
            {
                // TODO:提示用户不支持发布蓝牙服务
                return false;
            }

            serviceProvider.AdvertisementStatusChanged += ServiceProvider_AdvertisementStatusChanged;
            return true;
        }
        private static async Task<bool> CreateAppInfoCharacteristic()
        {
            var result = await serviceProvider.Service.CreateCharacteristicAsync
                (ServiceDefinition.AppInfoCharacteristic, ServiceDefinition.appInfoParameters);
            if (result.Error == BluetoothError.Success)
            {
                appInfoCharacteristic = result.Characteristic;
            }
            else
            {
                // TODO:提示用户不支持发布蓝牙特征
                return false;
            }

            appInfoCharacteristic.ReadRequested += AppInfoCharacteristic_ReadRequested;
            return true;
        }
        private static async Task<bool> CreateSendApplyCharacteristic()
        {
            var result = await serviceProvider.Service.CreateCharacteristicAsync
                (ServiceDefinition.ApplySendCharacteristic, ServiceDefinition.ApplySendParameters);
            if (result.Error == BluetoothError.Success)
            {
                applySendCharacteristic = result.Characteristic;
            }
            else
            {
                // TODO:提示用户不支持发布蓝牙特征
                return false;
            }

            applySendCharacteristic.WriteRequested += ApplySendCharacteristic_WriteRequested;
            return true;
        }
        private static async Task<bool> CreatePermitCharacteristic()
        {
            var result = await serviceProvider.Service.CreateCharacteristicAsync
                (ServiceDefinition.PermitCharacteristic, ServiceDefinition.permitParameters);
            if (result.Error == BluetoothError.Success)
            {
                permitCharacteristic = result.Characteristic;
            }
            else
            {
                // TODO:提示用户不支持发布蓝牙特征
                return false;
            }

            permitCharacteristic.SubscribedClientsChanged += PermitCharacteristic_SubscribedClientsChanged;
            return true;
        }
        private static void ServiceProvider_AdvertisementStatusChanged
            (GattServiceProvider sender, GattServiceProviderAdvertisementStatusChangedEventArgs args)
        {
            
        }
        private static async void AppInfoCharacteristic_ReadRequested
            (GattLocalCharacteristic sender, GattReadRequestedEventArgs args)
        {
            // BT_Code: Process a read request. 
            using (args.GetDeferral())
            {
                // Get the request information.  This requires device access before an app can access the device's request. 
                GattReadRequest request = await args.GetRequestAsync();
                if (request == null)
                {
                    // No access allowed to the device.  Application should indicate this to the user.
                    return;
                }

                var writer = new DataWriter();
                writer.ByteOrder = ByteOrder.LittleEndian;
                var str = "{hh}";
                writer.WriteBytes(Encoding.UTF8.GetBytes(str));

                // Gatt code to handle the response
                request.RespondWithValue(writer.DetachBuffer());
            }
        }
        private static void PermitCharacteristic_SubscribedClientsChanged
            (GattLocalCharacteristic sender, object args)
        {
            
        }
        private static async void ApplySendCharacteristic_WriteRequested
            (GattLocalCharacteristic sender, GattWriteRequestedEventArgs args)
        {
            var deferral = args.GetDeferral();

            var request = await args.GetRequestAsync();
            var reader = DataReader.FromBuffer(request.Value);
            // Parse data as necessary. 

            uint length = request.Value.Length;
            byte[] bytes = new byte[length];
            reader.ReadBytes(bytes);
            var str = Encoding.UTF8.GetString(bytes);

            if (request.Option == GattWriteOption.WriteWithResponse)
            {
                request.Respond();
            }

            deferral.Complete();
        }
    }
}
