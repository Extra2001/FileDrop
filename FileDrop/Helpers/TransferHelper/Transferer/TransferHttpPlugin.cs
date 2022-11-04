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
            var inPackagePath = Regex.Match
                (e.Context.Request.URL, "http://\\d+.\\d+.\\d+.\\d+:\\d+/").Result("");
            if (filePairs.TryGetValue(inPackagePath, out string path))
            {
                e.Context.Response
                    .SetStatus()
                    .FromFile(path, e.Context.Request);
            }
            else
            {
                e.Context.Response.SetStatus("404", "NotFound");
            }
            base.OnGet(client, e);
        }
    }
}
