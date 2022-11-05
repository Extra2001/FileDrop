using System;
using System.Collections.Generic;
using System.Diagnostics;
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

        public static async Task ResetWiFiAdapter()
        {
            var interfaces = NetworkInterface.GetAllNetworkInterfaces();
            var tasks = new List<Task>();
            foreach (var item in interfaces)
            {
                if (item.NetworkInterfaceType == NetworkInterfaceType.Wireless80211
                    && !item.Description.Contains("Direct"))
                {
                    var processInfo = new ProcessStartInfo
                    {
                        Verb = "runas",
                        FileName = "powershell.exe",
                        Arguments = $"Restart-NetAdapter -Name \\\"{item.Name}\\\"",
                        UseShellExecute = true,
                        CreateNoWindow = true,
                    };
                    tasks.Add(Process.Start(processInfo).WaitForExitAsync());
                }
            }
            await Task.WhenAll(tasks);
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

        public static void DisableTCPTurning()
        {
            var processInfo = new ProcessStartInfo
            {
                Verb = "runas",
                FileName = "powershell.exe",
                Arguments = $"netsh int tcp set global autotuninglevel=disabled",
                UseShellExecute = true,
                CreateNoWindow = true,
            };
            Process.Start(processInfo).WaitForExitAsync();
        }
    }
}
