using FileDrop.Helpers;
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

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace FileDrop.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class RecievePage : Page
    {
        public ObservableCollection<RecievedTransfer> transfers =
             new ObservableCollection<RecievedTransfer>();

        private Flyout openedFlyout;

        public RecievePage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            LoadRecieved();
        }

        public void LoadRecieved()
        {
            transfers.Clear();
            var collection = Repo.database.GetCollection<Transfer>();
            var find = collection.FindAll().OrderByDescending(x => x.StartTime);
            foreach (var item in find)
                transfers.Add(item.ToRecievedTransfer());
        }

        private void RefreshButtonState()
        {
            int checkedCount = transfers.Where(x => x.Checked).Count();

            if (checkedCount == 0)
            {
                selectAll.Visibility = Visibility.Visible;
                deselectAll.Visibility = Visibility.Collapsed;
                deleteSelect.IsEnabled = false;
            }
            else if (checkedCount == transfers.Count)
            {
                selectAll.Visibility = Visibility.Collapsed;
                deselectAll.Visibility = Visibility.Visible;
                deleteSelect.IsEnabled = true;
            }
            else
            {
                selectAll.Visibility = Visibility.Visible;
                deselectAll.Visibility = Visibility.Visible;
                deleteSelect.IsEnabled = true;
            }
        }

        private async void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            await Task.Delay(30);
            RefreshButtonState();
        }

        private void selectAll_Click(object sender, RoutedEventArgs e)
        {
            foreach (var item in transfers.Where(x => !x.Checked))
                item.Checked = true;

            Helpers.Dialog.ToastDialog.Show("Test");
        }
        
        private void deselectAll_Click(object sender, RoutedEventArgs e)
        {
            foreach (var item in transfers.Where(x => x.Checked))
                item.Checked = false;
        }

        private void deleteConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            var list = transfers.Where(x => x.Checked).ToList();
            var ids = list.Select(x => x.Id).ToList();
            var collection = Repo.database.GetCollection<Transfer>();
            collection.DeleteMany(x => ids.Contains(x.Id));
            foreach (var item in list)
                transfers.Remove(item);
            deleteConfirmFlyout.Hide();
        }

        private void deleteConfirmButton2_Click(object sender, RoutedEventArgs e)
        {
            int id = (int)((sender as Button).Tag);
            var item = transfers.Where(x => x.Id == id).FirstOrDefault();
            if (item != null)
            {
                var collection = Repo.database.GetCollection<Transfer>();
                collection.DeleteMany(x => x.Id == id);
                transfers.Remove(item);
            }
            openedFlyout?.Hide();
        }

        private void Flyout_Opened(object sender, object e)
        {
            openedFlyout = sender as Flyout;
        }
    }
}
