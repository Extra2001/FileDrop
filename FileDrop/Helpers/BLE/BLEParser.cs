using FileDrop.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage.Streams;

namespace FileDrop.Helpers.BLE
{
    public static class BLEParser
    {
        public static DataWriter GetAppInfoWriter()
        {
            var app = new AppInfo()
            {
                DeviceName = SettingsItem.GetSettings().LocalName
            };
            return GetWriter(app);
        }

        public static DataWriter GetWriter(object obj)
        {
            var writer = new DataWriter();
            writer.ByteOrder = ByteOrder.LittleEndian;
            var str = JsonConvert.SerializeObject(obj);
            writer.WriteBytes(Encoding.UTF8.GetBytes(str));
            return writer;
        }

        public static T Parse<T>(this IBuffer buffer)
        {
            byte[] bytes = new byte[buffer.Length];
            var reader = DataReader.FromBuffer(buffer);
            reader.ReadBytes(bytes);
            var str = Encoding.UTF8.GetString(bytes);
            return JsonConvert.DeserializeObject<T>(str);
        }
    }
}
