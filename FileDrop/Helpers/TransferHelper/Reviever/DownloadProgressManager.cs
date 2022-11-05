using FileDrop.Helpers.Dialog;
using FileDrop.Pages.Dialogs;
using FluentFTP;
using System;
using System.Threading.Tasks;

namespace FileDrop.Helpers.TransferHelper.Reviever
{
    public class DownloadProgressManager : IProgress<FtpProgress>
    {
        public void Report(FtpProgress value)
        {
            App.mainWindow.DispatcherQueue.TryEnqueue(() =>
            {
                try
                {
                    var content = ModelDialog.showedDialogs.dialog.Content as TransferProgressView;
                    content.UpdateProgress(value);
                }
                catch
                {
                    ModelDialog.ShowWaitingProgress("正在接收文件");
                }
            });
        }
    }
}
