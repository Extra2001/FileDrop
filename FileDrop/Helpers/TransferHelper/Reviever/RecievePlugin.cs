using FileDrop.Helpers.TransferHelper.Reciever;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TouchSocket.Rpc.TouchRpc;
using TouchSocket.Rpc.TouchRpc.Plugins;

namespace FileDrop.Helpers.TransferHelper.Reviever
{
    public class RecievePlugin : TouchRpcPluginBase
    {
        protected override void OnFileTransfering(ITouchRpc client, FileOperationEventArgs e)
        {
            RecieveStatusManager.manager.ReportFileOperator(e.FileOperator);
            base.OnFileTransfering(client, e);
        }

        protected override void OnFileTransfered(ITouchRpc client, FileTransferStatusEventArgs e)
        {
            base.OnFileTransfered(client, e);
            if (e.Metadata != null && e.Metadata.AllKeys.Contains("Zip"))
            {
                RestoreFile.RestoreZip(e.FileInfo.FilePath);
            }
        }
    }
}
