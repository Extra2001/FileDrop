using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.WiFiDirect;

namespace FileDrop.Helpers.WiFiDirect
{
    public class ConnectedDevice : IDisposable
    {
        public SocketReaderWriter SocketRW { get; }
        public WiFiDirectDevice WfdDevice { get; }
        public string DisplayName { get; }

        public ConnectedDevice(string displayName, WiFiDirectDevice wfdDevice, SocketReaderWriter socketRW)
        {
            DisplayName = displayName;
            WfdDevice = wfdDevice;
            SocketRW = socketRW;
        }

        public override string ToString() => DisplayName;

        public void Dispose()
        {
            // Close socket
            SocketRW.Dispose();
            // Close WiFiDirectDevice object
            WfdDevice.Dispose();
        }
    }
}
