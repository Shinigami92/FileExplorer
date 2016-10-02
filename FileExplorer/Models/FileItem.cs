using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.Storage;
using Windows.Storage.FileProperties;

namespace FileExplorer.Models
{
    public class FileItem
    {
        private static readonly IReadOnlyList<string> SizeEndings = new List<string>() { "B", "KB", "MB", "GB", "TB", "PB", "EB" };
        private static readonly IReadOnlyList<string> ImageFileTypes = new List<string>() { ".jpg", ".png", ".gif", ".bmp" };

        public IStorageItem StorageItem { get; set; }
        public bool IsFolder { get { return StorageItem.IsOfType(StorageItemTypes.Folder); } }
        public bool IsFile { get { return StorageItem.IsOfType(StorageItemTypes.File); } }
        public string Name { get { return StorageItem.Name; } }
        public string Icon { get { return IsFolder ? "\xE8B7" : "\xE7C3"; } }
        public string DateCreated { get { return StorageItem.DateCreated.ToString(); } }
        public string DateModified { get; private set; }
        public ulong Size { get; private set; }
        public StorageItemThumbnail Thumbnail { get; private set; }
        public string ToolTipText
        {
            get
            {
                var loader = ResourceLoader.GetForCurrentView();
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
                    result += tmpSize.ToString("0.00") + " " + SizeEndings.ElementAt(i);
                }
                return result;
            }
        }

        public FileItem(IStorageItem storageItem)
        {
            StorageItem = storageItem;
        }

        public async Task FetchProperties()
        {
            var properties = await StorageItem.GetBasicPropertiesAsync();
            DateModified = properties.DateModified.ToString();
            Size = properties.Size;
            if (IsFile)
            {
                var storageFile = StorageItem as StorageFile;
                var fileType = storageFile.FileType.ToLower();
                Debug.WriteLine(fileType);
                if (ImageFileTypes.Contains(fileType))
                {
                    Thumbnail = await storageFile.GetThumbnailAsync(ThumbnailMode.PicturesView, 110, ThumbnailOptions.UseCurrentScale);
                }
            }
        }
    }
}
