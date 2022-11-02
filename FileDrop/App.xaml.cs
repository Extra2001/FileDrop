using FileDrop.Helpers;
using FileDrop.Helpers.Dialog;
using FileDrop.Helpers.WiFiDirect;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI.Xaml.Shapes;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Foundation.Collections;

namespace FileDrop
{
    public partial class App : Application
    {
        public static MainWindow mainWindow { get; set; }

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
            WiFiDirectConnector.StopWatcher();

            await ModelDialog.ShowDialog("提示", "发生未捕获的异常" + e.Message);
            Exit();
        }
        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            mainWindow = new MainWindow();
            mainWindow.Activate();
        }
    }
}
