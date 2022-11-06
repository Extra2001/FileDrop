using FileDrop.Pages.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileDrop.Helpers.Dialog
{
    public class ToastDialog
    {
        public static void Show(string message)
        {
            App.mainWindow.DispatcherQueue.TryEnqueue(async () =>
            {
                var toast = new ToastView();
                toast.Message = message;
                App.mainWindow.ToastGrid.Children.Add(toast);
                await Task.Delay(3000);
                App.mainWindow.ToastGrid.Children.Remove(toast);
            });
        }
    }
}
