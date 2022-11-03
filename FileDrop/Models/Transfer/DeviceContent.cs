using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Devices.Enumeration;

namespace FileDrop.Models
{
    public class DeviceContent : INotifyPropertyChanged
    {
        public int Id { get; set; }
        public DeviceInformation deviceInfo { get; set; }
        public string deviceName
        {
            get => _deviceName;
            set
            {
                _deviceName = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(deviceName)));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private string _deviceName;

        public AppInfoView ToAppInfoView()
        {
            return new AppInfoView()
            {
                Id = Id,
                Checked = false,
                DeviceName = deviceName
            };
        }
    }
}
