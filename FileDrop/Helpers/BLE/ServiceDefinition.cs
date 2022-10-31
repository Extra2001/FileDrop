using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth.GenericAttributeProfile;

namespace FileDrop.Helpers.BLE
{
    public static class ServiceDefinition
    {
        public static Guid ServiceUUID => Guid.Parse("185CD6A1-3479-4A6D-9CD3-814B7D2DE481");
        public static Guid ApplySendCharacteristic => Guid.Parse("185CD6A1-3479-4A6D-9CD3-814B7D2DE482");
        public static Guid PermitCharacteristic => Guid.Parse("185CD6A1-3479-4A6D-9CD3-814B7D2DE483");
        public static Guid AppInfoCharacteristic => Guid.Parse("185CD6A1-3479-4A6D-9CD3-814B7D2DE484");

        public static readonly GattLocalCharacteristicParameters ApplySendParameters = new GattLocalCharacteristicParameters
        {
            CharacteristicProperties = GattCharacteristicProperties.Write,
            WriteProtectionLevel = GattProtectionLevel.EncryptionRequired,
            UserDescription = "Apply Send Characteristic"
        };

        public static readonly GattLocalCharacteristicParameters appInfoParameters = new GattLocalCharacteristicParameters
        {
            CharacteristicProperties = GattCharacteristicProperties.Read,
            WriteProtectionLevel = GattProtectionLevel.EncryptionRequired,
            UserDescription = "App Info Characteristic"
        };

        public static readonly GattLocalCharacteristicParameters permitParameters = new GattLocalCharacteristicParameters
        {
            CharacteristicProperties = GattCharacteristicProperties.Notify,
            WriteProtectionLevel = GattProtectionLevel.EncryptionRequired,
            UserDescription = "Permit Characteristic"
        };
    }
}
