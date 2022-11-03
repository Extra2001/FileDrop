using FileDrop.Helpers.Dialog;
using FileDrop.Helpers.TransferHelper.Schemas;
using FileDrop.Helpers.WiFiDirect;
using FileDrop.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;

namespace FileDrop.Helpers.TransferHelper.Tranferer
{
    public class TransferTask
    {
        private SocketReaderWriter socket;
        private TransferInfo transferInfo;
        private Transfer transfer;

        public async Task<Transfer> StartTransfer(SocketReaderWriter socket, TransferInfo transferInfo)
        {
            this.socket = socket;
            this.transferInfo = transferInfo;

            TransferStatusManager.StartNew(transferInfo.TransferInfos.Count);

            await socket.WriteAsync(transferInfo);
            var res = await socket.ReadWithRetryAsync(60);
            var respond = JsonConvert.DeserializeObject<TransferRespond>(res.info);

            if (respond.Recieve == false)
            {
                TransferStatusManager.manager.ReportError(true, "对方拒绝了共享请求");
                return null;
            }

            TransferStatusManager.StartNew(transferInfo.TransferInfos.Count);

            transfer = new Transfer()
            {
                DirectoryName = null,
                StartTime = DateTimeOffset.Now,
                TransferInfos = transferInfo.TransferInfos,
                FileInfos = transferInfo.FileInfos,
                From = null,
                To = transferInfo.deviceName,
                TransferDirection = TransferDirection.Transfer
            };

            for (int i = 0; i < transferInfo.TransferInfos.Count; i++)
            {
                TransferStatusManager.manager.ReportTransferItem(i);
                await TransferItem(transferInfo.TransferInfos[i]);
            }

            transfer.EndTime = DateTimeOffset.Now;
            var collection = Repo.database.GetCollection<Transfer>();
            collection.Insert(transfer);
            TransferStatusManager.manager.ReportDone();
            return transfer;
        }

        private async Task TransferItem(TransferItem item)
        {
            uint packSize = 104857600;
            var path = item.Path;
            var length = new FileInfo(path).Length;
            var file = await StorageFile.GetFileFromPathAsync(path);

            using var stream = await file.OpenSequentialReadAsync();
            using var reader = new DataReader(stream);

            int total = (int)(length / packSize) + 1;

            for (int index = 0; index < total; index++)
            {
                var packInfo = new TransferPackageInfo()
                {
                    Index = index,
                    Total = total
                };
                IBuffer data;
                var l = await reader.LoadAsync(packSize);
                data = reader.ReadBuffer(l);
                TransferStatusManager.manager.ReportPack(index, total);
                await socket.WriteAsync(packInfo, data);
                await socket.ReadAsync();
            }
        }
    }
}
