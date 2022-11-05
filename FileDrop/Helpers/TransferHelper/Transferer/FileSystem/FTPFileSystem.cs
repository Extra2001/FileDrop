using FileDrop.Models;
using FubarDev.FtpServer.BackgroundTransfer;
using FubarDev.FtpServer.FileSystem;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage.Provider;

namespace FileDrop.Helpers.TransferHelper.Transferer.FileSystem
{
    public class FTPFileSystem : IUnixFileSystem
    {
        public static readonly int DefaultStreamBufferSize = 4096;

        private List<TransferItem> transferItems;

        public bool SupportsAppend => false;
        public bool SupportsNonEmptyDirectoryDelete => false;
        public StringComparer FileSystemEntryComparer => StringComparer.OrdinalIgnoreCase;
        public IUnixDirectoryEntry Root => FTPDirectoryEntry.Root;

        public FTPFileSystem(TransferInfo transferInfo)
        {
            transferItems = transferInfo.TransferInfos;
        }

        public Task<IReadOnlyList<IUnixFileSystemEntry>> GetEntriesAsync(IUnixDirectoryEntry directoryEntry, CancellationToken cancellationToken)
        {
            var result = new List<IUnixFileSystemEntry>();

            if (directoryEntry.IsRoot)
            {
                var dirList = new Dictionary<string, string>();
                foreach (var item in transferItems)
                {
                    var dir = Path.GetDirectoryName(item.InPackagePath);
                    if (string.IsNullOrEmpty(dir))
                    {
                        result.Add(new FTPFileEntry(new FileInfo(item.Path)));
                    }
                    else
                    {
                        var rootDir = dir.Split('\\', '/')[0];
                        var fullDir = item.Path.Replace(item.InPackagePath, "");
                        dirList.TryAdd(rootDir, Path.Combine(fullDir, rootDir));
                    }
                }
                foreach (var item in dirList)
                {
                    result.Add(new FTPDirectoryEntry(new DirectoryInfo(item.Value)));
                }
            }
            else
            {
                var searchDirInfo = ((FTPDirectoryEntry)directoryEntry).DirectoryInfo;
                foreach (var info in searchDirInfo.EnumerateFileSystemInfos())
                {
                    if (info is DirectoryInfo dirInfo)
                        result.Add(new FTPDirectoryEntry(dirInfo));
                    else
                    {
                        if (info is FileInfo fileInfo)
                            result.Add(new FTPFileEntry(fileInfo));
                    }
                }
            }

            return Task.FromResult<IReadOnlyList<IUnixFileSystemEntry>>(result);
        }
        public Task<IUnixFileSystemEntry> GetEntryByNameAsync(IUnixDirectoryEntry directoryEntry, string name, CancellationToken cancellationToken)
        {
            IUnixFileSystemEntry result = null;

            if (directoryEntry.IsRoot)
            {
                foreach (var item in transferItems)
                {
                    var dir = Path.GetDirectoryName(item.InPackagePath);
                    if (string.IsNullOrEmpty(dir))
                    {
                        if (name == item.InPackagePath)
                        {
                            result = new FTPFileEntry(new FileInfo(item.Path));
                            break;
                        }
                    }
                    else
                    {
                        var rootDir = dir.Split('\\', '/')[0];
                        if (name == rootDir)
                        {
                            var fullDir = item.Path.Replace(item.InPackagePath, "");
                            result = new FTPDirectoryEntry(new DirectoryInfo
                                (Path.Combine(fullDir, rootDir)));
                            break;
                        }
                    }
                }
            }
            else
            {
                var searchDirInfo = ((FTPDirectoryEntry)directoryEntry).Info;
                var fullPath = Path.Combine(searchDirInfo.FullName, name);

                if (File.Exists(fullPath))
                {
                    result = new FTPFileEntry(new FileInfo(fullPath));
                }
                else if (Directory.Exists(fullPath))
                {
                    result = new FTPDirectoryEntry(new DirectoryInfo(fullPath));
                }
            }

            return Task.FromResult(result);
        }
        public Task<Stream> OpenReadAsync(IUnixFileEntry fileEntry, long startPosition, CancellationToken cancellationToken)
        {
            var fileInfo = ((FTPFileEntry)fileEntry).FileInfo;
            var input = new FileStream(fileInfo.FullName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            if (startPosition != 0)
            {
                input.Seek(startPosition, SeekOrigin.Begin);
            }

            return Task.FromResult<Stream>(input);
        }

        public Task<IUnixFileSystemEntry> MoveAsync(IUnixDirectoryEntry parent, IUnixFileSystemEntry source, IUnixDirectoryEntry target, string fileName, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
        public Task UnlinkAsync(IUnixFileSystemEntry entry, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
        public Task<IUnixDirectoryEntry> CreateDirectoryAsync(IUnixDirectoryEntry targetDirectory, string directoryName, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
        public Task<IBackgroundTransfer> AppendAsync(IUnixFileEntry fileEntry, long? startPosition, Stream data, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
        public Task<IBackgroundTransfer> CreateAsync(IUnixDirectoryEntry targetDirectory, string fileName, Stream data, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
        public Task<IBackgroundTransfer> ReplaceAsync(IUnixFileEntry fileEntry, Stream data, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
        public Task<IUnixFileSystemEntry> SetMacTimeAsync(IUnixFileSystemEntry entry, DateTimeOffset? modify, DateTimeOffset? access, DateTimeOffset? create, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
