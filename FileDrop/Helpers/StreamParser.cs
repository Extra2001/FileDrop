using FileDrop.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage.Streams;

namespace FileDrop.Helpers
{
    public static class StreamParser
    {
        public static DataWriter GetAppInfoWriter()
        {
            var app = new AppInfo()
            {
                DeviceName = SettingsItem.GetSettings().LocalName
            };
            return GetWriter(app);
        }

        public static DataWriter GetWriter(string str)
        {
            var writer = new DataWriter();
            writer.ByteOrder = ByteOrder.LittleEndian;
            writer.WriteBytes(Encoding.UTF8.GetBytes(str));
            return writer;
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
            return JsonConvert.DeserializeObject<T>(buffer.Parse());
        }

        public static string Parse(this IBuffer buffer)
        {
            byte[] bytes = new byte[buffer.Length];
            var reader = DataReader.FromBuffer(buffer);
            reader.ReadBytes(bytes);
            return Encoding.UTF8.GetString(bytes);
        }
    }
}
