using FileDrop.Helpers;
using FileDrop.Helpers.BLE;
using FileDrop.Helpers.WiFiDirect;
using FileDrop.Models;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;

namespace FileDrop.Pages
{
    public sealed partial class TransferPage : Page
    {
        public ObservableCollection<ToSendFile> toSendFiles = new ObservableCollection<ToSendFile>();

        public ObservableCollection<AppInfoView> deviceContents
            = new ObservableCollection<AppInfoView>();

        public TransferPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            LoadToSendFile();
            InitWiFiDirect();
        }
        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            //BLEClient.ScanComplete -= BLEClient_ScanComplete;
            //BLEClient.StopBleDeviceWatcher();
            WiFiDirectConnector.ScanComplete -= WiFiDirectConnector_ScanComplete;
            WiFiDirectConnector.StopWatcher();
        }
        private void InitWiFiDirect()
        {
            WiFiDirectConnector.deviceContents.CollectionChanged += DeviceContents_CollectionChanged;
            WiFiDirectConnector.StartWatcher();
            WiFiDirectConnector.ScanComplete += WiFiDirectConnector_ScanComplete;
        }

        private void WiFiDirectConnector_ScanComplete()
        {
            
        }

        private void InitBLE()
        {
            BLEClient.deviceContents.CollectionChanged += DeviceContents_CollectionChanged;
            BLEClient.StartBleDeviceWatcher();
            BLEClient.ScanComplete += BLEClient_ScanComplete;
        }
        private void DeviceContents_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
            {
                var item = e.NewItems[0] as DeviceContent;
                item.PropertyChanged += DeviceContent_PropertyChanged;
                deviceContents.Add(item.ToAppInfoView());
            }
            else if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Reset)
            {
                deviceContents.Clear();
            }
            else if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove)
            {
                var item = e.OldItems[0] as DeviceContent;
                item.PropertyChanged -= DeviceContent_PropertyChanged;
                var find = deviceContents.Where(x => x.Id == item.Id).FirstOrDefault();
                if (find != null)
                    deviceContents.Remove(find);
            }
        }
        private void DeviceContent_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            var dc = sender as DeviceContent;
            var find = deviceContents.Where(x => x.Id == dc.Id).FirstOrDefault();
            if (find != null)
                find.DeviceName = dc.deviceName;
        }
        private void BLEClient_ScanComplete()
        {
            
        }

        private async void addFileButton_Click(object sender, RoutedEventArgs e)
        {
            var picker = new Windows.Storage.Pickers.FileOpenPicker();
            picker.ViewMode = Windows.Storage.Pickers.PickerViewMode.List;
            picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.Desktop;
            picker.FileTypeFilter.Add("*");
            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.mainWindow);
            WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);
            var files = await picker.PickMultipleFilesAsync();

            foreach (var file in files)
            {
                toSendFiles.Add(new ToSendFile()
                {
                    Path = file.Path,
                    Name = file.DisplayName,
                    FileType = Symbol.OpenFile,
                    Id = toSendFiles.Count + 1,
                });
            }

            SetBorder();
            SaveToSendFile();
        }

        private void SetBorder()
        {
            foreach (var item in toSendFiles)
                item.BorderVisibility = Visibility.Visible;
            var last = toSendFiles.LastOrDefault();
            if (last != null) last.BorderVisibility = Visibility.Collapsed;
        }

        private async void addDirButton_Click(object sender, RoutedEventArgs e)
        {
            var picker = new Windows.Storage.Pickers.FolderPicker();
            picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.Desktop;
            picker.FileTypeFilter.Add("*");
            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.mainWindow);
            WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);
            Windows.Storage.StorageFolder folder = await picker.PickSingleFolderAsync();

            if (folder != null)
            {
                toSendFiles.Add(new ToSendFile()
                {
                    Path = folder.Path,
                    Name = folder.DisplayName,
                    FileType = Symbol.Folder,
                    Id = toSendFiles.Count + 1,
                    BorderVisibility = Visibility.Collapsed
                });
            }

            SetBorder();
            SaveToSendFile();
        }

        private void SaveToSendFile()
        {
            TempStorage.ToSendFiles = toSendFiles.ToList();
        }
        private void LoadToSendFile()
        {
            toSendFiles.Clear();
            foreach (var item in TempStorage.ToSendFiles)
                toSendFiles.Add(item);
        }

        private async void sendButton_Click(object sender, RoutedEventArgs e)
        {
            var info = await PrepareFile.Prepare(toSendFiles);
        }
    }
}
