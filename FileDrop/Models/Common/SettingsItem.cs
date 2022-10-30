using FileDrop.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace FileDrop.Models
{
    public class SettingsItem
    {
        public int Id { get; set; } = 1;
        public string LocalName { get; set; } = Environment.UserName;
        public RecieveFolder RecieveFolder { get; set; } = RecieveFolder.Downloads;
        public string RecievePath { get; set; } = DirectoryHelper.GetDownloadsPath();

        public SettingsItemView ToSettingsItemView()
        {
            return new SettingsItemView()
            {
                LocalName = LocalName,
                RecieveFolderDownload = RecieveFolder == RecieveFolder.Downloads,
                RecieveFolderCustomize = RecieveFolder == RecieveFolder.Customize,
                RecievePath = RecievePath
            };
        }
        public static SettingsItem GetSettings()
        {
            var collection = Repo.database.GetCollection<SettingsItem>();
            var settings = collection.FindAll().FirstOrDefault();
            if (settings == null)
            {
                settings = new SettingsItem();
                collection.Insert(settings);
            }
            return settings;
        }
        public void Save()
        {
            var collection = Repo.database.GetCollection<SettingsItem>();
            collection.Upsert(this);
        }
    }
}
