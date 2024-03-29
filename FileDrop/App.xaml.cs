﻿using FileDrop.Helpers;
using FileDrop.Helpers.Dialog;
using FileDrop.Helpers.TransferHelper.Reviever;
using FileDrop.Helpers.WiFiDirect.Advertiser;
using FileDrop.Helpers.WiFiDirect.Connector;
using Microsoft.UI.Xaml;

namespace FileDrop
{
    public partial class App : Application
    {
        public static MainWindow mainWindow { get; set; }
        private NotificationManager notificationManager;

        public App()
        {
            this.InitializeComponent();
            Repo.InitializeDatabase();
            UnhandledException += App_UnhandledException;
        }
        private async void App_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            e.Handled = true;
            Repo.SaveAndClose();
            if (WiFiDirectAdvertiser.Started)
                WiFiDirectAdvertiser.StopAdvertisement();
            WiFiDirectAdvertiser.CloseDevice();
            WiFiDirectConnector.StopWatcher();
            WiFiDirectConnector.CloseDevice();
            RecieveTask.StopWaitForTransfer();
            notificationManager.Unregister();
            await ModelDialog.ShowDialog("提示", "发生未捕获的异常" + e.Message);
            Exit();
        }
        protected override void OnLaunched(LaunchActivatedEventArgs args)
        {
            notificationManager = new NotificationManager();
            notificationManager.Init();
            mainWindow = new MainWindow();
            mainWindow.Activate();
        }
    }
}
