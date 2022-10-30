using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace FileDrop.Models
{
    public class TransferFile
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string InPackagePath { get; set; }
        public FileType FileType { get; set; }

        public RecievedFile ToRecievedFile(string directoryName, bool borderVisible)
        {
            return new RecievedFile()
            {
                Id = Id,
                Name = Name,
                Path = System.IO.Path.Combine(directoryName, InPackagePath),
                BorderVisibility = borderVisible ? Visibility.Visible : Visibility.Collapsed,
                FileTypeString = FileType == FileType.Files ? "文件" : "文件夹",
                FileType = FileType == FileType.Files ? Symbol.OpenFile : Symbol.Folder
            };
        }
    }
}
