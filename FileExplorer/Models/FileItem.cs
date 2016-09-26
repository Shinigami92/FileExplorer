using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace FileExplorer.Models
{
    public class FileItem
    {
        public IStorageItem StorageItem { get; set; }

        public FileItem(IStorageItem storageItem)
        {
            this.StorageItem = storageItem;
        }

        public bool IsFolder { get { return this.StorageItem.IsOfType(StorageItemTypes.Folder); } }
        public bool IsFile { get { return this.StorageItem.IsOfType(StorageItemTypes.File); } }
        public string Name { get { return this.StorageItem.Name; } }
        public string DateCreated { get { return this.StorageItem.DateCreated.ToString(); } }
    }
}
