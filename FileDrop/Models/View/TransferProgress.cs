using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileDrop.Models.View
{
    public class TransferProgress : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public string speed
        {
            get => _speed;
            set
            {
                _speed = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(speed)));
            }
        }
        public string progress
        {
            get => _progress;
            set
            {
                _progress = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(progress)));
            }
        }
        public string ETA
        {
            get => _ETA;
            set
            {
                _ETA = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ETA)));
            }
        }

        private string _speed;
        private string _progress;
        private string _ETA;
    }
}
