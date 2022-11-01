using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileDrop.Models
{
    public class TransferInfo
    {
        public string deviceName { get; set; }
        public List<TransferItem> TransferInfos { get; set; }
        public List<TransferFile> FileInfos { get; set; }
    }
}
