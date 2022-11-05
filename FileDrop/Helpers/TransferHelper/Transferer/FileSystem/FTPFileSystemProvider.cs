using FubarDev.FtpServer.FileSystem;
using FubarDev.FtpServer;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using FileDrop.Models;

namespace FileDrop.Helpers.TransferHelper.Transferer.FileSystem
{
    public class FTPFileSystemProvider : IFileSystemClassFactory
    {
        private readonly TransferInfo transferInfo;

        public FTPFileSystemProvider(IOptions<FTPFileSystemOptions> options)
        {
            transferInfo = options.Value.transferInfo;
        }

        public Task<IUnixFileSystem> Create(IAccountInformation accountInformation)
        {
            return Task.FromResult<IUnixFileSystem>(new FTPFileSystem(transferInfo));
        }
    }
}
