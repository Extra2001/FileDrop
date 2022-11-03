using FileDrop.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace FileDrop.Helpers
{
    public class DirectoryHelper
    {
        [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
        static extern int SHGetKnownFolderPath
            ([MarshalAs(UnmanagedType.LPStruct)] Guid rfid, uint dwFlags, IntPtr hToken, out string pszPath);

        public static string GetDownloadsPath()
        {
            string downloads;
            SHGetKnownFolderPath(new Guid("374DE290-123F-4565-9164-39C4925E467B"), 0, IntPtr.Zero, out downloads);
            return downloads;
        }

        public static string GetRecievePath()
        {
            var settings = SettingsItem.GetSettings();
            if (settings.RecieveFolder == RecieveFolder.Downloads)
                return GetDownloadsPath();
            return settings.RecievePath;
        }

        public static string GenerateRecieveFolder(string deviceName)
        {
            var path = GetRecievePath();
            var name = deviceName + "_" + DateTime.Now.ToString("s");
            var folder = Path.Combine(path, name);
            Directory.CreateDirectory(folder);
            return folder;
        }
    }
}
