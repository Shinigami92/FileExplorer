using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
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
		public bool IsFolder { get => this.StorageItem.IsOfType(StorageItemTypes.Folder); }
		public bool IsFile { get => this.StorageItem.IsOfType(StorageItemTypes.File); }
		public string Name { get => this.StorageItem.Name; }
		public string Icon { get => this.IsFolder ? "\xE8B7" : "\xE7C3"; }
		public string DateCreated { get => this.StorageItem.DateCreated.ToString(); }
		public string DateModified { get; private set; }
		public ulong Size { get; private set; }
		public StorageItemThumbnail Thumbnail { get; private set; }
		public string ToolTipText
		{
			get
			{
				var loader = ResourceLoader.GetForCurrentView();
				var result = $"{this.Name}\n{loader.GetString("FileItem_Created")}: {this.DateCreated}\n{loader.GetString("FileItem_Modified")}: {this.DateModified}";
				if (this.Size != 0)
				{
					result += $"\n{loader.GetString("FileItem_Size")}: ";
					double tmpSize = this.Size;
					var i = 0;
					while (tmpSize > 1024)
					{
						tmpSize /= 1024;
						i++;
					}
					result += $"{tmpSize.ToString("0.00")} {SizeEndings.ElementAt(i)}";
				}
				return result;
			}
		}

		public FileItem(IStorageItem storageItem)
		{
			this.StorageItem = storageItem;
		}

		public async Task FetchProperties()
		{
			var properties = await this.StorageItem.GetBasicPropertiesAsync();
			this.DateModified = properties.DateModified.ToString();
			this.Size = properties.Size;
			if (this.IsFile)
			{
				var storageFile = this.StorageItem as StorageFile;
				var fileType = storageFile.FileType.ToLower();
				Debug.WriteLine(fileType);
				if (ImageFileTypes.Contains(fileType))
				{
					this.Thumbnail = await storageFile.GetThumbnailAsync(ThumbnailMode.PicturesView, 110, ThumbnailOptions.UseCurrentScale);
				}
			}
		}
	}
}
