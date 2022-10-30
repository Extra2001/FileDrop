using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileDrop.Models
{
    public class RecievedTransfer : INotifyPropertyChanged
    {
        private int _Id;
        private string _From;
        private string _DirectoryName;
        private ObservableCollection<RecievedFile> _FileInfos;
        private string _Time;
        private string _UsedTime;
        private bool _Checked;

        public int Id
        {
            get => _Id;
            set
            {
                _Id = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Id)));
            }
        }
        public string From
        {
            get => _From;
            set
            {
                _From = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(From)));
            }
        }
        public string DirectoryName
        {
            get => _DirectoryName;
            set
            {
                _DirectoryName = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DirectoryName)));
            }
        }
        public ObservableCollection<RecievedFile> FileInfos
        {
            get => _FileInfos;
            set
            {
                _FileInfos = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FileInfos)));
            }
        }
        public string Time
        {
            get => _Time;
            set
            {
                _Time = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Time)));
            }
        }
        public string UsedTime
        {
            get => _UsedTime;
            set
            {
                _UsedTime = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(UsedTime)));
            }
        }
        public bool Checked
        {
            get => _Checked;
            set
            {
                _Checked = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Checked)));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
