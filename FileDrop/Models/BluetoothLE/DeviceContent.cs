using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Devices.Enumeration;

namespace FileDrop.Models.BluetoothLE
{
    public class DeviceContent
    {
        public DeviceInformation deviceInfo { get; set; }
        public string deviceName { get; set; }
    }
}
