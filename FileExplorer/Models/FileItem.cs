using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.UI.Xaml.Controls;

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
        public string Icon { get { return this.IsFolder ? "\xE8B7" : "\xE7C3"; } }
        public string DateCreated { get { return this.StorageItem.DateCreated.ToString(); } }
        public string DateModified { get; private set; }
        public ulong Size { get; private set; }
        public StorageItemThumbnail Thumbnail { get; private set; }

        private static List<string> SIZE_ENDINGS = new List<string>() { "B", "KB", "MB", "GB", "TB" };
        private static List<string> IMAGE_FILE_TYPES = new List<string>() { ".jpg", ".png" };

        public string ToolTipText
        {
            get
            {
                ResourceLoader loader = ResourceLoader.GetForCurrentView();
                string result = string.Format("{0}\n{1}: {2}\n{3}: {4}", Name, loader.GetString("FileItem_Created"), DateCreated, loader.GetString("FileItem_Modified"), DateModified);
                if (Size != 0)
                {
                    result += "\n" + loader.GetString("FileItem_Size") + ": ";
                    double tmpSize = Size;
                    int i = 0;
                    while (tmpSize > 1024)
                    {
                        tmpSize /= 1024;
                        i++;
                    }
                    result += tmpSize.ToString("0.00") + " " + SIZE_ENDINGS.ElementAt(i);
                }
                return result;
            }
        }

        public async Task FetchProperties()
        {
            BasicProperties properties = await this.StorageItem.GetBasicPropertiesAsync();
            DateModified = properties.DateModified.ToString();
            Size = properties.Size;
            if (IsFile)
            {
                StorageFile storageFile = StorageItem as StorageFile;
                Debug.WriteLine(storageFile.FileType);
                if (IMAGE_FILE_TYPES.Contains(storageFile.FileType))
                {
                    Thumbnail = await storageFile.GetThumbnailAsync(ThumbnailMode.PicturesView, 110, ThumbnailOptions.UseCurrentScale);
                }
            }
        }
    }
}
