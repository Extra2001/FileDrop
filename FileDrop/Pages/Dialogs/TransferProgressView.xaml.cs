using FileDrop.Models.View;
using FluentFTP;
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
using Windows.Foundation;
using Windows.Foundation.Collections;

namespace FileDrop.Pages.Dialogs
{
    public sealed partial class TransferProgressView : Page
    {
        public TransferProgress transferProgress = new TransferProgress();
        public TransferProgressView()
        {
            this.InitializeComponent();
        }

        public void UpdateProgress(FtpProgress progress)
        {
            transferProgress.progress = progress.Progress.ToString("0.00");
            transferProgress.ETA = $"{(int)progress.ETA.TotalMinutes}分{progress.ETA.Seconds}秒";
            transferProgress.speed = progress.TransferSpeedToString();
        }
    }
}
