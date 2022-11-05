using FubarDev.FtpServer.FileSystem;
using System;
using System.IO;

namespace FileDrop.Helpers.TransferHelper.Transferer.FileSystem
{
    public class FTPFileEntry : FTPFileSystemEntry, IUnixFileEntry
    {
        public FTPFileEntry(FileInfo info) : base(info)
        {
            FileInfo = info;
        }

        public FileInfo FileInfo { get; }

        public long Size => FileInfo.Length;
    }
}
