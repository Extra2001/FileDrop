using FileDrop.Helpers;
using FileDrop.Helpers.Dialog;
using FileDrop.Helpers.TransferHelper;
using FileDrop.Helpers.TransferHelper.Transferer;
using FileDrop.Helpers.WiFiDirect;
using FileDrop.Helpers.WiFiDirect.Connector;
using FileDrop.Models;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage.Streams;

namespace FileDrop.Pages
{
    public sealed partial class TransferPage : Page
    {
        public ObservableCollection<ToSendFile> toSendFiles = new ObservableCollection<ToSendFile>();
        public ObservableCollection<AppInfoView> deviceContents = new ObservableCollection<AppInfoView>();

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
            CloseWiFiDirect();
        }

        #region WiFiDirectConfigure
        private void InitWiFiDirect()
        {
            WiFiDirectConnector.deviceContents.CollectionChanged += DeviceContents_CollectionChanged;
            WiFiDirectConnector.ScanStart += WiFiDirectConnector_ScanStart;
            WiFiDirectConnector.ScanComplete += WiFiDirectConnector_ScanComplete;
            WiFiDirectConnector.StartWatcher();
        }
        private void CloseWiFiDirect()
        {
            WiFiDirectConnector.deviceContents.CollectionChanged -= DeviceContents_CollectionChanged;
            WiFiDirectConnector.ScanStart -= WiFiDirectConnector_ScanStart;
            WiFiDirectConnector.ScanComplete -= WiFiDirectConnector_ScanComplete;
            WiFiDirectConnector.StopWatcher();
        }
        private void WiFiDirectConnector_ScanStart()
        {
            App.mainWindow.DispatcherQueue.TryEnqueue(() =>
            {
                scaningDevice.Visibility = Visibility.Visible;
                scaningDevice.IsActive = true;
                refreshButton.IsEnabled = false;
            });
        }
        private void WiFiDirectConnector_ScanComplete()
        {
            WiFiDirectConnector.StopWatcher();
            App.mainWindow.DispatcherQueue.TryEnqueue(() =>
            {
                scaningDevice.Visibility = Visibility.Collapsed;
                scaningDevice.IsActive = false;
                refreshButton.IsEnabled = true;
            });
        }
        private void DeviceContents_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            App.mainWindow.DispatcherQueue.TryEnqueue(() =>
            {
                if (e.Action == NotifyCollectionChangedAction.Add)
                {
                    var item = e.NewItems[0] as DeviceContent;
                    item.PropertyChanged += DeviceContent_PropertyChanged;
                    deviceContents.Add(item.ToAppInfoView());
                }
                else if (e.Action == NotifyCollectionChangedAction.Reset)
                {
                    deviceContents.Clear();
                }
                else if (e.Action == NotifyCollectionChangedAction.Remove)
                {
                    var item = e.OldItems[0] as DeviceContent;
                    item.PropertyChanged -= DeviceContent_PropertyChanged;
                    var find = deviceContents.Where(x => x.Id == item.Id).FirstOrDefault();
                    if (find != null)
                        deviceContents.Remove(find);
                }
                SetSendButtonEnabled();
            });
        }
        private void DeviceContent_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            App.mainWindow.DispatcherQueue.TryEnqueue(() =>
            {
                var dc = sender as DeviceContent;
                var find = deviceContents.Where(x => x.Id == dc.Id).FirstOrDefault();
                if (find != null)
                    find.DeviceName = dc.deviceName;
                SetSendButtonEnabled();
            });
        }
        #endregion

        #region SelectFiles
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
        private void deleteButton_Click(object sender, RoutedEventArgs e)
        {
            int id = (int)(sender as Button).Tag;
            toSendFiles.Remove(toSendFiles.Where(x => x.Id == id).FirstOrDefault());
        }
        #endregion

        #region SelectDevice
        private void ToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            foreach (var item in deviceContents)
                if (item.Id != (int)(sender as ToggleButton).Tag)
                    item.Checked = false;
            SetSendButtonEnabled();
        }
        private void ToggleButton_Unchecked(object sender, RoutedEventArgs e)
        {
            SetSendButtonEnabled();
        }
        private async void SetSendButtonEnabled()
        {
            await Task.Delay(50);
            sendButton.IsEnabled = deviceContents.Where(x => x.Checked).Any();
        }
        private void refreshButton_Click(object sender, RoutedEventArgs e)
        {
            WiFiDirectConnector.StartWatcher();
        }
        #endregion

        private async void sendButton_Click(object sender, RoutedEventArgs e)
        {
            var apv = deviceContents.Where(x => x.Checked).FirstOrDefault();
            if (apv == null)
            {
                _ = ModelDialog.ShowDialog("提示", "未选择设备");
                return;
            }
            var dc = WiFiDirectConnector.deviceContents.Where(x => x.Id == apv.Id).FirstOrDefault();

            ModelDialog.ShowWaiting("正在准备文件", $"正在准备要发送的文件");
            var info = await PrepareFile.Prepare(toSendFiles);
            WiFiDirectConnector.ConnectDevice(dc.deviceInfo, wfdDevice =>
            {
                if (wfdDevice != null)
                {
                    TransferTask.RequestTransfer(wfdDevice.
                        GetConnectionEndpointPairs()[0].RemoteHostName, info);
                }
            });
        }
    }
}
