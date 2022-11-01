using FileDrop.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileDrop.Helpers
{
    public static class TempStorage
    {
        public static List<ToSendFile> ToSendFiles { get; set; } = new List<ToSendFile>();
        public static bool Advertising { get; set; } = true;
    }
}
