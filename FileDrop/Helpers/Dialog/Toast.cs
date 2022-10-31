using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileDrop.Helpers.Dialog
{
    public class Toast
    {
        public static async void Show(string message)
        {
            var toast = new Pages.Dialogs.Toast();
            toast.Message = message;
            App.mainWindow.ToastGrid.Children.Add(toast);
            await Task.Delay(3000);
            App.mainWindow.ToastGrid.Children.Remove(toast);
        }
    }
}
