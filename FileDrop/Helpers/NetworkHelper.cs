using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace FileDrop.Helpers
{
    public class NetworkHelper
    {
        public static int GetRandomPort()
        {
            var random = new Random();
            var randomPort = random.Next(10000, 65535);

            while (IPGlobalProperties.GetIPGlobalProperties().GetActiveTcpListeners().Any(p => p.Port == randomPort))
            {
                randomPort = random.Next(10000, 65535);
            }

            return randomPort;
        }
    }
}
