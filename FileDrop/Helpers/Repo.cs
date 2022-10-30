using FileDrop.Models;
using LiteDB;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace FileDrop.Helpers
{
    public static class Repo
    {
        public static LiteDatabase database;

        public static void InitializeDatabase()
        {
            var local = ApplicationData.Current.LocalFolder.Path;
            var dir = Path.Combine(local, "Database");
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);
            var path = Path.Combine(dir, "database.db");
            database = new LiteDatabase(path);
            InitializeSettings();

            //var collection = database.GetCollection<Transfer>();
            //collection.Insert(new Transfer()
            //{
            //    DirectoryName = "testhhh",
            //    StartTime = DateTimeOffset.Now - TimeSpan.FromSeconds(5),
            //    EndTime = DateTimeOffset.Now,
            //    From = "jxt",
            //    To = "jxt",
            //    TransferDirection = TransferDirection.Recieve,
            //    TransferInfos = new List<TransferItem>()
            //           {
            //               new TransferItem()
            //               {
            //                    InPackagePath = "555.txt",
            //                     MD5 = "",
            //                      TransferType = TransferType.File
            //               }
            //           },
            //    FileInfos = new List<TransferFile>()
            //     {
            //         new TransferFile()
            //         {
            //              InPackagePath  ="555.txt",
            //               FileType = FileType.Files,
            //                Id = 1,
            //                 Name = "555.txt"
            //         },new TransferFile()
            //         {
            //              InPackagePath  ="555.txt",
            //               FileType = FileType.Files,
            //                Id = 1,
            //                 Name = "555.txt"
            //         },new TransferFile()
            //         {
            //              InPackagePath  ="555.txt",
            //               FileType = FileType.Files,
            //                Id = 1,
            //                 Name = "555.txt"
            //         },new TransferFile()
            //         {
            //              InPackagePath  ="555.txt",
            //               FileType = FileType.Files,
            //                Id = 1,
            //                 Name = "555.txt"
            //         },new TransferFile()
            //         {
            //              InPackagePath  ="555.txt",
            //               FileType = FileType.Files,
            //                Id = 1,
            //                 Name = "555.txt"
            //         },new TransferFile()
            //         {
            //              InPackagePath  ="555.txt",
            //               FileType = FileType.Files,
            //                Id = 1,
            //                 Name = "555.txt"
            //         },new TransferFile()
            //         {
            //              InPackagePath  ="555.txt",
            //               FileType = FileType.Files,
            //                Id = 1,
            //                 Name = "555.txt"
            //         },new TransferFile()
            //         {
            //              InPackagePath  ="555.txt",
            //               FileType = FileType.Files,
            //                Id = 1,
            //                 Name = "555.txt"
            //         },
            //     }
            //});
        }

        public static void Save()
        {
            if (database == null) return;
            database.Commit();
        }

        public static void SaveAndClose()
        {
            if (database == null) return;
            database.Dispose();
        }

        public static void InitializeSettings()
        {
            var collection = database.GetCollection<SettingsItem>();
            if (!collection.FindAll().Any())
                collection.Upsert(new SettingsItem());
        }
    }
}
