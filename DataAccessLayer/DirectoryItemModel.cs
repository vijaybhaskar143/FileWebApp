using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessLayer
{
    public class DirectoryItemModel
    {
        public DirectoryItemModel() { }
        public DirectoryItemModel(string name, string location, string fullPath, string tag, PathPermissionsModel permissions, FileItemModel[] files, DirectoryItemModel[] directories) { }
        
        public string Path { get; }
        public string FullPath { get; set; }
        public string Location { get; set; }
        public DirectoryItemModel[] Directories { get; set; }
        public FileItemModel[] Files { get; set; }
    }
}
