using FileDrop.Models;
using FubarDev.FtpServer.FileSystem;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime;
using System.Text;
using System.Threading.Tasks;

namespace FileDrop.Helpers.TransferHelper.Transferer.FileSystem
{
    public class FTPDirectoryEntry : FTPFileSystemEntry, IUnixDirectoryEntry
    {
        public static FTPDirectoryEntry Root => new FTPDirectoryEntry();

        public DirectoryInfo DirectoryInfo { get; }

        public bool IsRoot { get; }

        /// <inheritdoc/>
        public bool IsDeletable => false;

        private FTPDirectoryEntry()
            : base(new DirectoryInfo(Path.GetTempPath()))
        {
            IsRoot = true;
        }

        public FTPDirectoryEntry(DirectoryInfo dirInfo) : base(dirInfo)
        {
            IsRoot = false;
            DirectoryInfo = dirInfo;
        }
    }
}
