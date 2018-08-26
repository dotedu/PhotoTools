using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhotoTools
{
    public class FileInfos
    {

    }
    public class DirectoryRecord
    {
        public DirectoryInfo Info { get; set; }

        public IEnumerable<FileInfo> Files
        {
            get
            {
                return Info.GetFiles().Where(t => t.Extension.ToLower().EndsWith(".jpg")|| t.Extension.ToLower().EndsWith(".jpeg") || t.Extension.ToLower().EndsWith(".bmp") || t.Extension.ToLower().EndsWith(".png"));
            }
        }



        public IEnumerable<DirectoryRecord> Directories
        {
            get
            {
                try
                {
                    return from di in Info.GetDirectories("*", SearchOption.TopDirectoryOnly).Where(i => !IsSystemHidden(i))
                           select new DirectoryRecord { Info = di };
                }
                catch (Exception)
                {
                    return null;
                }
            }
        }
        private static bool IsSystemHidden(DirectoryInfo dirInfo)
        {
            if (dirInfo.Parent == null)
            {
                return false;
            }
            string attributes = dirInfo.Attributes.ToString();
            if (attributes.IndexOf("Hidden") > -1 && attributes.IndexOf("System") > -1)
            {
                return true;
            }
            return false;
        }
    }
}
