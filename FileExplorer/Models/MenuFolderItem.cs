using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace FileExplorer.Models
{
    public class MenuFolderItem
    {
        public StorageFolder Folder { get; set; }

        public MenuFolderItem(StorageFolder folder)
        {
            this.Folder = folder;
        }

        public string DisplayName { get { return this.Folder.DisplayName; } }
        public string Icon { get { return this.Folder.Path.Length <= 3 ? "\xEDA2" : "\xE8B7"; } }
    }
}
