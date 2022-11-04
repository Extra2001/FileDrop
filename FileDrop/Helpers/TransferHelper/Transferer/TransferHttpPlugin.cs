using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TouchSocket.Http;
using TouchSocket.Http.Plugins;
using TouchSocket.Sockets;

namespace FileDrop.Helpers.TransferHelper.Transferer
{
    public class TransferHttpPlugin : HttpPluginBase
    {
        private Dictionary<string, string> filePairs = new Dictionary<string, string>();

        public void AddFile(string path, string inPackagePath)
        {
            filePairs.Add(inPackagePath, path);
        }

        protected override void OnGet(ITcpClientBase client, HttpContextEventArgs e)
        {
            var inPackagePath = e.Context.Request.URL.Remove(0, 1);
            if (filePairs.TryGetValue(inPackagePath, out string path))
            {
                try
                {
                    e.Context.Response
                        .SetStatus()
                        .FromFile(path, e.Context.Request);
                }
                catch { }
            }
            else
            {
                try
                {
                    e.Context.Response.SetStatus("404", "NotFound");
                }
                catch { }
            }
            base.OnGet(client, e);
        }
    }
}
