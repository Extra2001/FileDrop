using FileDrop.Helpers.Dialog;
using FileDrop.Helpers.TransferHelper.Reciever;
using FileDrop.Helpers.TransferHelper.Schemas;
using FileDrop.Helpers.WiFiDirect;
using FileDrop.Models;
using Microsoft.UI.Xaml.Controls;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Windows.Storage;
using Windows.Storage.Streams;

namespace FileDrop.Helpers.TransferHelper.Reviever
{
    public class RecieveTask
    {
        private SocketReaderWriter socket;
        private TransferInfo transferInfo;
        private Transfer transfer;

        public async Task<Transfer> StartRecieve(SocketReaderWriter socket)
        {
            this.socket = socket;
            var first = await socket.ReadAsync();
            transferInfo = JsonConvert.DeserializeObject<TransferInfo>(first.info);
            var dres = await ModelDialog.ShowDialog("开始传输",
                $"{transferInfo.deviceName}想要共享{transferInfo.FileInfos.Count}个文件（夹）", "接受", "取消");
            if (dres == Microsoft.UI.Xaml.Controls.ContentDialogResult.Primary)
            {
                await socket.WriteAsync(new TransferRespond() { Recieve = true });

                RecieveStatusManager.StartNew(transferInfo.TransferInfos.Count);
                var folder = DirectoryHelper.GenerateRecieveFolder(transferInfo.deviceName);
                transfer = new Transfer()
                {
                    DirectoryName = folder,
                    StartTime = DateTimeOffset.Now,
                    TransferInfos = transferInfo.TransferInfos,
                    FileInfos = transferInfo.FileInfos,
                    From = transferInfo.deviceName,
                    To = null,
                    TransferDirection = TransferDirection.Recieve
                };

                for (int i = 0; i < transferInfo.TransferInfos.Count; i++)
                {
                    RecieveStatusManager.manager.ReportTransferItem(i);
                    await RecieveItem(transferInfo.TransferInfos[i]);
                }

                RestoreFile.RestoreZip(Path.Combine(transfer.DirectoryName, "package.zip"));

                transfer.EndTime = DateTimeOffset.Now;
                var collection = Repo.database.GetCollection<Transfer>();
                collection.Insert(transfer);
                RecieveStatusManager.manager.ReportDone();
                return transfer;
            }
            else
            {
                await socket.WriteAsync(new TransferRespond() { Recieve = false });
                return null;
            }
        }

        private async Task RecieveItem(TransferItem item)
        {
            StorageFile file;
            if (item.TransferType == TransferType.File)
            {
                var path = Path.Combine(transfer.DirectoryName, item.InPackagePath);
                var dir = Path.GetDirectoryName(path);
                var name = Path.GetFileName(path);
                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);
                var folder = await StorageFolder.GetFolderFromPathAsync(dir);
                file = await folder.CreateFileAsync(name);
            }
            else
            {
                var folder = await StorageFolder.GetFolderFromPathAsync(transfer.DirectoryName);
                file = await folder.CreateFileAsync("package.zip");
            }
            using var stream = await file.OpenAsync(FileAccessMode.ReadWrite, StorageOpenOptions.AllowReadersAndWriters);
            using var writer = new DataWriter(stream);

            int total = 1, index = 0;
            while (index < total)
            {
                var rec = await socket.ReadAsync();
                var packInfo = JsonConvert.DeserializeObject<TransferPackageInfo>(rec.info);
                total = packInfo.Total;
                index = packInfo.Index;
                RecieveStatusManager.manager.ReportPack(index, total);
                index++;
                writer.WriteBuffer(rec.payload);
                await writer.StoreAsync();
                await writer.FlushAsync();
                GC.Collect();
                await socket.WriteAsync(new TransferRespond() { Recieve = true });
            }
        }
    }
}
