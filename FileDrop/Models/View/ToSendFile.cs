using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace FileDrop.Models
{
    public class ToSendFile : INotifyPropertyChanged
    {
        private int _Id;
        private string _Name;
        private string _Path;
        private Symbol _FileType;
        private Visibility _BorderVisibility;

        public int Id
        {
            get => _Id;
            set
            {
                _Id = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Id)));
            }
        }
        public string Name
        {
            get => _Name;
            set
            {
                _Name = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Name)));
            }
        }
        public string Path
        {
            get => _Path;
            set
            {
                _Path = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(_Path)));
            }
        }
        public Symbol FileType
        {
            get => _FileType;
            set
            {
                _FileType = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FileType)));
            }
        }
        public Visibility BorderVisibility
        {
            get => _BorderVisibility;
            set
            {
                _BorderVisibility = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(BorderVisibility)));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public TransferFile ToTransferFile()
        {
            return new TransferFile()
            {
                Id = Id,
                Name = Name,
                FileType = FileType == Symbol.Folder ? Models.FileType.Directory : Models.FileType.Files,
                InPackagePath = System.IO.Path.GetFileName(Path)
            };
        }
    }
}
