using FileDrop.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Devices.WiFiDirect;
using Windows.Security.Cryptography;
using Windows.Storage.Streams;

namespace FileDrop.Helpers.WiFiDirect
{
    public static class ConnectDefinition
    {
        public static readonly byte[] AppOui = { 0x8B, 0xA6, 0x18 };
        public static readonly byte AppInfoOuiType = 0x47;
        public static readonly byte DeviceNameOuiType = 0x83;
        public static readonly string strServerPort = "50001";

        public static List<WiFiDirectInformationElement> GetInformationElements()
        {
            var ret = new List<WiFiDirectInformationElement>();

            var informationElement = new WiFiDirectInformationElement();
            var dataWriter = StreamParser.GetWriter("FileDrop v1.0.0");
            informationElement.Value = dataWriter.DetachBuffer();
            informationElement.Oui = CryptographicBuffer.CreateFromByteArray(AppOui);
            informationElement.OuiType = AppInfoOuiType;
            ret.Add(informationElement);

            informationElement = new WiFiDirectInformationElement();
            dataWriter = StreamParser.GetWriter(SettingsItem.GetSettings().LocalName);
            informationElement.Value = dataWriter.DetachBuffer();
            informationElement.Oui = CryptographicBuffer.CreateFromByteArray(AppOui);
            informationElement.OuiType = DeviceNameOuiType;
            ret.Add(informationElement);

            return ret;
        }
    }
}
