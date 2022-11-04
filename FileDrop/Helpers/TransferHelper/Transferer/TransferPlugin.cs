using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TouchSocket.Rpc.TouchRpc;
using TouchSocket.Rpc.TouchRpc.Plugins;

namespace FileDrop.Helpers.TransferHelper.Transferer
{
    public class TransferPlugin : TouchRpcPluginBase
    {
        protected override void OnFileTransfering(ITouchRpc client, FileOperationEventArgs e)
        {
            e.FileOperator.Timeout = -1;
            base.OnFileTransfering(client, e);
        }

        protected override void OnFileTransfered(ITouchRpc client, FileTransferStatusEventArgs e)
        {
            base.OnFileTransfered(client, e);
        }
    }
}
