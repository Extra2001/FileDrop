using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;

namespace FileDrop.Models
{
    public class Transfer
    {
        public int Id { get; set; }
        public string From { get; set; }
        public string To { get; set; }
        public string DirectoryName { get; set; }
        public List<TransferItem> TransferInfos { get; set; }
        public List<TransferFile> FileInfos { get; set; }
        public TransferDirection TransferDirection { get; set; }
        public DateTimeOffset StartTime { get; set; }
        public DateTimeOffset EndTime { get; set; }

        public RecievedTransfer ToRecievedTransfer()
        {
            var ret = new RecievedTransfer()
            {
                Id = Id,
                From = From,
                FileInfos = new ObservableCollection<RecievedFile>(),
                DirectoryName = DirectoryName,
                Time = StartTime.ToLocalTime().ToString("g"),
                UsedTime = (EndTime - StartTime).ToString("mm\\:ss")
            };
            var list = FileInfos.OrderBy(x => x.Id);
            var last = list.LastOrDefault();
            foreach (var item in list)
                ret.FileInfos.Add(item.ToRecievedFile(DirectoryName, item != last));
            return ret;
        }
    }
}
