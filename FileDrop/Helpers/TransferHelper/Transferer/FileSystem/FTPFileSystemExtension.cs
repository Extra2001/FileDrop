using FubarDev.FtpServer.FileSystem;
using FubarDev.FtpServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using FileDrop.Models;

namespace FileDrop.Helpers.TransferHelper.Transferer.FileSystem
{
    public static class FTPFileSystemExtension
    {
        public static IFtpServerBuilder UseFTPFileSystem(this IFtpServerBuilder builder)
        {
            builder.Services.AddSingleton<IFileSystemClassFactory, FTPFileSystemProvider>();
            return builder;
        }
    }
}
