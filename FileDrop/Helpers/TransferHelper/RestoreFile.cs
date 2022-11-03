using ICSharpCode.SharpZipLib.Zip;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileDrop.Helpers.TransferHelper
{
    public class RestoreFile
    {
        public static void RestoreZip(string path)
        {
            new FastZip().ExtractZip(path, Path.GetDirectoryName(path),
                FastZip.Overwrite.Always, null, null, null, true);
            File.Delete(path);
        }
    }
}
