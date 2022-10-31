using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth;

namespace FileDrop.Helpers.BLE
{
    public class BLECheck
    {
        public static async Task<bool> CheckComeptivity()
        {
            var peripheralSupported = await CheckPeripheralRoleSupportAsync();
            return peripheralSupported;
        }

        private static async Task<bool> CheckPeripheralRoleSupportAsync()
        {
            // BT_Code: New for Creator's Update - Bluetooth adapter has properties of the local BT radio.
            var localAdapter = await BluetoothAdapter.GetDefaultAsync();

            if (localAdapter != null)
            {
                return localAdapter.IsPeripheralRoleSupported;
            }
            else
            {
                // Bluetooth is not turned on 
                return false;
            }
        }
    }
}
