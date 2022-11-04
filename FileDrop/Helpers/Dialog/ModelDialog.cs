using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileDrop.Helpers.Dialog
{
    public class ModelDialog
    {
        public static ShowedDialog showedDialogs = null;

        public static async Task<ContentDialogResult> ShowDialog
            (string title, string content, string primaryButton = "确定", string closeButton = null)
        {
            if (showedDialogs != null)
                showedDialogs.Hide();

            ContentDialog dialog = new ContentDialog();

            dialog.XamlRoot = App.mainWindow.Content.XamlRoot;
            dialog.Title = title;
            dialog.PrimaryButtonText = primaryButton;
            dialog.CloseButtonText = closeButton;
            dialog.DefaultButton = ContentDialogButton.Primary;
            dialog.Content = content;
            showedDialogs = new ShowedDialog()
            {
                dialog = dialog
            };
            return await dialog.ShowAsync();
        }

        public static void ShowWaiting(string title, string content)
        {
            App.mainWindow.DispatcherQueue.TryEnqueue(() =>
            {
                if (showedDialogs != null)
                    showedDialogs.Hide();
                ContentDialog dialog = new ContentDialog();
                dialog.XamlRoot = App.mainWindow.Content.XamlRoot;
                dialog.Title = title;
                dialog.DefaultButton = ContentDialogButton.Primary;
                dialog.Content = content;
                try
                {
                    _ = dialog.ShowAsync();
                    showedDialogs = new ShowedDialog()
                    {
                        dialog = dialog
                    };
                }
                catch { }
            });
        }
    }
}
