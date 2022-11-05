using FileDrop.Helpers;
using FileDrop.Helpers.WiFiDirect.Advertiser;
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
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;

namespace FileDrop.Pages
{
    public sealed partial class SettingsPage : Page
    {
        public SettingsItemView settings { get; set; }
        public SettingsPage()
        {
            this.InitializeComponent();
        }
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            settings = SettingsItem.GetSettings().ToSettingsItemView();
        }

        private void saveButton_Click(object sender, RoutedEventArgs e)
        {
            settings.ToSettingsItem().Save();
        }

        private void turnOffTCPTurningButton_Click(object sender, RoutedEventArgs e)
        {
            NetworkHelper.DisableTCPTurning();
        }
        private async void resetAdapterButton_Click(object sender, RoutedEventArgs e)
        {
            await NetworkHelper.ResetWiFiAdapter();
        }
    }
}
