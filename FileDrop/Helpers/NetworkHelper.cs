using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using WindowsFirewallHelper;

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

        public static void ResetWiFiAdapter()
        {
            var interfaces = NetworkInterface.GetAllNetworkInterfaces();

            foreach (var item in interfaces)
            {
                if (item.Name.Contains("WLAN") ||
                    item.NetworkInterfaceType == NetworkInterfaceType.Wireless80211)
                {
                    var processInfo = new ProcessStartInfo
                    {
                        Verb = item.Description.Contains("Direct") ? "" : "runas",
                        FileName = "powershell.exe",
                        Arguments = "Restart-NetAdapter -Name \"WLAN\"",
                        UseShellExecute = true
                    };

                    Process.Start(processInfo);
                }
            }
        }

        public static void SetNetworkProfileToPrivate()
        {
            //var rule = FirewallManager.Instance.CreateApplicationRule(
            //      @"FileDrop Rule",
            //      FirewallAction.Allow,
            //      Environment.ProcessPath
            //  );
            //rule.Direction = FirewallDirection.Inbound;
            //FirewallManager.Instance.Rules.Add(rule);
        }
    }
}
