using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileDrop.Models
{
    public class AppInfoView : INotifyPropertyChanged
    {
        public string DeviceName
        {
            get => _DeviceName;
            set
            {
                _DeviceName = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DeviceName)));
            }
        }
        public int Id
        {
            get => _id;
            set
            {
                _id = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Id)));
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

        private string _DeviceName;
        private int _id;
        private bool _Checked;

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
