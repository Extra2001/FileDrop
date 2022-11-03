using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileDrop.Models.Transfer
{
    public class TransferRespond
    {
        public bool Recieve { get; set; }
        public int Port { get; set; }
        public string Token { get; set; }
    }
}
