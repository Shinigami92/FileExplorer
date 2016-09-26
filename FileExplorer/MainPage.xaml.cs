using FileExplorer.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.AccessCache;
using Windows.Storage.FileProperties;
using Windows.Storage.Pickers;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// Die Vorlage "Leere Seite" ist unter http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409 dokumentiert.

namespace FileExplorer
{
    /// <summary>
    /// Eine leere Seite, die eigenständig verwendet oder zu der innerhalb eines Rahmens navigiert werden kann.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private StorageFolder currentFolder = null;
        private ObservableCollection<MenuFolderItem> MenuFolderItems = new ObservableCollection<MenuFolderItem>();
        private ObservableCollection<FileItem> FileItems = new ObservableCollection<FileItem>();

        public MainPage()
        {
            this.InitializeComponent();
            StorageApplicationPermissions.FutureAccessList.Entries.ToList().ForEach(e => Debug.WriteLine("Metadata: " + e.Metadata + " Token: " + e.Token));
            //StorageApplicationPermissions.FutureAccessList.Clear();
        }

        private void MenuButtonMainLeft_Click(object sender, RoutedEventArgs e)
        {
            MenuSplitViewMainLeft.IsPaneOpen = !MenuSplitViewMainLeft.IsPaneOpen;
        }

        private async void FileListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            FileItem fi = e.ClickedItem as FileItem;
            IStorageItem storageItem = fi.StorageItem;
            Debug.WriteLine("[FileListView_ItemClick] Clicked on: " + fi.Name);
            if (fi.IsFolder)
            {
                StorageFolder folder = storageItem as StorageFolder;
                FileItems.Clear();
                IReadOnlyList<IStorageItem> folderItems = await folder.GetItemsAsync();
                foreach (IStorageItem folderItem in folderItems)
                {
                    FileItems.Add(new FileItem(folderItem));
                }
                currentFolder = folder;
            }
            else
            {
                Debug.WriteLine("Name: " + storageItem.Name);
                Debug.WriteLine("Path: " + storageItem.Path);
                Debug.WriteLine("DateCreated: " + storageItem.DateCreated);
                BasicProperties properties = await storageItem.GetBasicPropertiesAsync();
                Debug.WriteLine("DateModified: " + properties.DateModified);
                Debug.WriteLine("ItemDate: " + properties.ItemDate);
                Debug.WriteLine("Size: " + properties.Size);
            }
        }

        private async void MenuButtonMainAddFolder_ItemClick(object sender, ItemClickEventArgs e)
        {
            FolderPicker folderPicker = new FolderPicker();
            folderPicker.FileTypeFilter.Add("*");
            folderPicker.SuggestedStartLocation = PickerLocationId.ComputerFolder;
            folderPicker.ViewMode = PickerViewMode.List;
            StorageFolder folder = await folderPicker.PickSingleFolderAsync();
            if (folder != null)
            {
                StorageApplicationPermissions.FutureAccessList.Add(folder, folder.Path);
                Debug.WriteLine("Opened the folder: " + folder.DisplayName);
                MenuFolderItems.Add(new MenuFolderItem(folder));
            }
        }

        private void MenuListViewItemRemove_Click(object sender, RoutedEventArgs e)
        {
            MenuFlyoutItem source = e.OriginalSource as MenuFlyoutItem;
            Debug.WriteLine("source.Tag: " + source.Tag);
            StorageFolder storageFolder = source.Tag as StorageFolder;
            MenuFolderItem f = MenuFolderItems.ToList().Find(item => item.Folder == storageFolder);
            AccessListEntry entry = StorageApplicationPermissions.FutureAccessList.Entries.ToList().Find(item => item.Metadata == f.Folder.Path);
            StorageApplicationPermissions.FutureAccessList.Remove(entry.Token);
            MenuFolderItems.Remove(f);
            Debug.WriteLine("Removed FolderItem with Name: " + entry.Metadata + " Token: " + entry.Token);
        }

        private async void MenuListViewFolders_Loaded(object sender, RoutedEventArgs e)
        {
            foreach (AccessListEntry entry in StorageApplicationPermissions.FutureAccessList.Entries.OrderBy(item => item.Metadata))
            {
                StorageFolder folder = await StorageFolder.GetFolderFromPathAsync(entry.Metadata);
                StorageApplicationPermissions.FutureAccessList.Add(folder, folder.Path);
                Debug.WriteLine("Opened the folder: " + folder.DisplayName);
                MenuFolderItems.Add(new MenuFolderItem(folder));
            }
        }

        private async void MenuListViewFolders_ItemClick(object sender, ItemClickEventArgs e)
        {
            MenuFolderItem f = e.ClickedItem as MenuFolderItem;
            Debug.WriteLine("[MenuListViewFolders_ItemClick] Clicked on: " + f.DisplayName);
            FileItems.Clear();
            IReadOnlyList<IStorageItem> folderItems = await f.Folder.GetItemsAsync();
            foreach (IStorageItem folderItem in folderItems)
            {
                FileItems.Add(new FileItem(folderItem));
            }
            currentFolder = f.Folder;
        }

        private async void FolderUpButton_Click(object sender, RoutedEventArgs e)
        {
            if (currentFolder != null)
            {
                StorageFolder parentFolder = await currentFolder.GetParentAsync();
                if (parentFolder != null)
                {
                    FileItems.Clear();
                    IReadOnlyList<IStorageItem> folderItems = await parentFolder.GetItemsAsync();
                    foreach (IStorageItem folderItem in folderItems)
                    {
                        FileItems.Add(new FileItem(folderItem));
                    }
                    currentFolder = parentFolder;
                }
                else
                {
                    Debug.WriteLine("Cant access parent folder");
                }
            }
            else
            {
                Debug.WriteLine("There was no currentFolder");
            }
        }
    }
}
