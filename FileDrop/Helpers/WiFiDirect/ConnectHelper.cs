using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Devices.WiFiDirect;
using Windows.Foundation;
using Windows.UI.Core;

namespace FileDrop.Helpers.WiFiDirect
{
    public static class ConnectHelper
    {
        public static async Task<bool> RequestPairDeviceAsync(DeviceInformationPairing pairing)
        {
            WiFiDirectConnectionParameters connectionParams = new WiFiDirectConnectionParameters();
            DevicePairingKinds devicePairingKinds = DevicePairingKinds.ConfirmOnly;

            connectionParams.PreferredPairingProcedure = WiFiDirectPairingProcedure.Invitation;
            DeviceInformationCustomPairing customPairing = pairing.Custom;
            customPairing.PairingRequested += OnPairingRequested;

            DevicePairingResult result = await customPairing.PairAsync(devicePairingKinds, DevicePairingProtectionLevel.Default, connectionParams);
            if (result.Status != DevicePairingResultStatus.Paired)
            {
                return false;
            }
            return true;
        }

        private static void OnPairingRequested
            (DeviceInformationCustomPairing sender, DevicePairingRequestedEventArgs args)
        {
            using (Deferral deferral = args.GetDeferral())
            {
                args.Accept();
            }
        }
    }
}
