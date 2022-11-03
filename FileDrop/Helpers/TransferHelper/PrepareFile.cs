using FileDrop.Models;
using ICSharpCode.SharpZipLib.Zip;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.System;
using Windows.UI.Xaml;

namespace FileDrop.Helpers.TransferHelper
{
    public static class PrepareFile
    {
        public static long FileLengthThreshold = 100 * 1024 * 1024;

        public static async Task<TransferInfo> Prepare(IEnumerable<ToSendFile> files)
        {
            List<TransferFile> transferFiles = new List<TransferFile>();
            foreach (var item in files)
                transferFiles.Add(item.ToTransferFile());
            var transferItems = await PrepareTransferItems(files);
            var transferInfo = new TransferInfo
            {
                TransferInfos = transferItems,
                FileInfos = transferFiles,
                deviceName = SettingsItem.GetSettings().LocalName
            };
            return transferInfo;
        }

        public static async Task<List<TransferItem>> PrepareTransferItems(IEnumerable<ToSendFile> files)
        {
            return await Task.Run(() =>
            {
                List<TransferItem> transferItems = new List<TransferItem>();

                var local = ApplicationData.Current.TemporaryFolder.Path;
                if (!Directory.Exists(local))
                    Directory.CreateDirectory(local);

                var zipPath = Path.Combine(local, "package.zip");
                if (File.Exists(zipPath)) File.Delete(zipPath);

                long entryCount;
                using (ZipFile zip = ZipFile.Create(zipPath))
                {
                    zip.BeginUpdate();

                    foreach (var item in files)
                    {
                        if (item.FileType == Symbol.OpenFile)
                            AddFile(item.Path, Path.GetDirectoryName(item.Path), zip, transferItems);
                        else
                            AddFolder(item.Path, Path.GetDirectoryName(item.Path), zip, transferItems);
                    }
                    entryCount = zip.Count;
                    zip.CommitUpdate();
                }

                if (entryCount > 0)
                {
                    transferItems.Add(new TransferItem()
                    {
                        Id = GetId(transferItems),
                        InPackagePath = "",
                        TransferType = TransferType.Zip,
                        Path = zipPath
                    });
                }

                return transferItems;
            });
        }

        private static void AddFile(string path, string dir, ZipFile zip, List<TransferItem> transferItems)
        {
            var fileInfo = new FileInfo(path);
            if (fileInfo.Length > FileLengthThreshold)
            {
                // 不压缩直接传输
                transferItems.Add(new TransferItem()
                {
                    Id = GetId(transferItems),
                    InPackagePath = GetInPackagePath(dir, path),
                    TransferType = TransferType.File,
                    Path = path
                });
            }
            else
            {
                // 需要压缩
                zip.Add(path, GetInPackagePath(dir, path));
            }
        }

        private static void AddFolder(string path, string dir, ZipFile zip, List<TransferItem> transferItems)
        {
            var files = Directory.GetFiles(path);
            foreach (var file in files)
                AddFile(file, dir, zip, transferItems);

            var dirs = Directory.GetDirectories(path);
            foreach (var d in dirs)
                AddFolder(d, dir, zip, transferItems);
        }

        private static int GetId(List<TransferItem> transferItems)
        {
            if (transferItems.Any())
                return transferItems.Select(x => x.Id).Max() + 1;
            else return 1;
        }

        private static string GetInPackagePath(string dir, string path)
        {
            var ret = path.Replace(dir + "\\", "");
            ret = ret.Replace("\\", "/");
            return ret;
        }
    }
}
