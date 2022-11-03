using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileDrop.Helpers.TransferHelper
{
    public class SpeedParser
    {
        public static string Parse(long speed)
        {
            if (speed < 1024)
                return $"{speed} b/s";
            if (speed < (1024 * 1024))
                return $"{speed / 1024.0} Kb/s";
            return $"{speed / 1024.0 / 1024.0} Mb/s";
        }
    }
}
