using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml;

namespace FileDrop.Models
{
    public class RecievedFile : INotifyPropertyChanged
    {
        private int _Id;
        private string _Name;
        private string _Path;
        private string _FileTypeString;
        private Visibility _BorderVisibility;
        private Symbol _FileType;
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
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(_Name)));
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
        public string FileTypeString
        {
            get => _FileTypeString;
            set
            {
                _FileTypeString = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(_FileTypeString)));
            }
        }
        public Visibility BorderVisibility
        {
            get => _BorderVisibility;
            set
            {
                _BorderVisibility = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(_BorderVisibility)));
            }
        }
        public Symbol FileType
        {
            get => _FileType;
            set
            {
                _FileType = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(_FileType)));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
