using FileDrop.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileDrop.Models
{
    public class SettingsItemView : INotifyPropertyChanged
    {
        public string _LocalName;
        public bool _RecieveFolderDownload;
        public bool _RecieveFolderCustomize;
        public string _RecievePath;

        public string LocalName
        {
            get => _LocalName;
            set
            {
                _LocalName = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(LocalName)));
            }
        }
        public bool RecieveFolderDownload
        {
            get => _RecieveFolderDownload;
            set
            {
                _RecieveFolderDownload = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(RecieveFolderDownload)));
            }
        }

        public bool RecieveFolderCustomize
        {
            get => _RecieveFolderCustomize;
            set
            {
                _RecieveFolderCustomize = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(RecieveFolderCustomize)));
            }
        }

        public string RecievePath
        {
            get => _RecievePath;
            set
            {
                _RecievePath = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(RecievePath)));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public SettingsItem ToSettingsItem()
        {
            return new SettingsItem()
            {
                LocalName = LocalName,
                RecieveFolder = RecieveFolderDownload ? RecieveFolder.Downloads : RecieveFolder.Customize,
                RecievePath = RecievePath
            };
        }
    }
}
