using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileDrop.Helpers.Dialog
{
    public class ShowedDialog
    {
        public ContentDialog dialog { get; set; }
        public void Hide()
        {
            ModelDialog.showedDialogs = null;
            dialog.Hide();
        }
    }
}
