using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileDrop.Models
{
    public class TransferItem
    {
        public int Id { get; set; }
        public string MD5 { get; set; }
        public string Path { get; set; }
        public string InPackagePath { get; set; }
        public TransferType TransferType { get; set; }
    }
}
