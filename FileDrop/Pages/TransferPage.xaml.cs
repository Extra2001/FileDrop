using FileDrop.Helpers;
using FileDrop.Helpers.BLE;
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

        public TransferPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            LoadToSendFile();
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

            if (await BLEServer.InitServiceAsync())
                BLEServer.StartServer();

            if (BLEServer.Started)
                BLEServer.StopServer();
            BLEClient.StartBleDeviceWatcher();
        }
    }
}
