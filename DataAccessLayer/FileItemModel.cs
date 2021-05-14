using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessLayer
{
    public class FileItemModel
    {
        public FileItemModel() { }
        public FileItemModel(string name, string extension, long length, string location, string url, string tag, PathPermissionsModel permissions) { }
        public string Extension { get; set; }
        public long Length { get; set; }
        public string Path { get; }
        public string Location { get; set; }
        public string Url { get; set; }
    }
}
