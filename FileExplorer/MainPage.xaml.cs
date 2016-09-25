using System;
using System.Collections.Generic;
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

        public MainPage()
        {
            this.InitializeComponent();

            StorageApplicationPermissions.FutureAccessList.Entries.ToList().ForEach(e => Debug.WriteLine("Metadata: " + e.Metadata + " Token: " + e.Token));
            //StorageApplicationPermissions.FutureAccessList.Clear();

            //ItemCollection fileListViewItems = FileListView.Items;
            //Debug.WriteLine("FileListViewItems.Count: " + fileListViewItems.Count);
            //fileListViewItems.Clear();

            //for (int i = 0; i < 100; i++)
            //{
            //    fileListViewItems.Add(BuildFileListViewItem("Column 01", "Column 02"));
            //}
        }

        private ListViewItem BuildFileListViewItem(IStorageItem storageItem)
        {
            Grid itemGrid = new Grid();
            itemGrid.Tag = storageItem;
            itemGrid.ColumnDefinitions.Add(new ColumnDefinition());
            itemGrid.ColumnDefinitions.Add(new ColumnDefinition());
            TextBlock tbLeft = new TextBlock();
            tbLeft.Text = storageItem.Name;
            Grid.SetColumn(tbLeft, 0);
            itemGrid.Children.Add(tbLeft);
            TextBlock tbRight = new TextBlock();
            tbRight.Text = storageItem.DateCreated.ToString();
            Grid.SetColumn(tbRight, 1);
            itemGrid.Children.Add(tbRight);
            ListViewItem item = new ListViewItem();
            item.Content = itemGrid;
            item.HorizontalContentAlignment = HorizontalAlignment.Stretch;
            item.VerticalContentAlignment = VerticalAlignment.Center;

            return item;
        }

        private void MenuButtonMainLeft_Click(object sender, RoutedEventArgs e)
        {
            MenuSplitViewMainLeft.IsPaneOpen = !MenuSplitViewMainLeft.IsPaneOpen;
        }

        private async void FileListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            Grid item = (Grid)e.ClickedItem;
            IStorageItem storageItem = (IStorageItem)item.Tag;
            Debug.WriteLine("[FileListView_ItemClick] Clicked on: " + storageItem.Name);
            if (storageItem.Attributes.HasFlag(Windows.Storage.FileAttributes.Directory))
            {
                StorageFolder folder = (StorageFolder)storageItem;
                FileListView.Items.Clear();
                IReadOnlyList<IStorageItem> folderItems = await folder.GetItemsAsync();
                foreach (IStorageItem folderItem in folderItems)
                {
                    FileListView.Items.Add(BuildFileListViewItem(folderItem));
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

                MenuListViewFolders.Items.Add(BuildMenuListViewItem("\xEDA2", folder.DisplayName, folder));
            }
        }

        private ListViewItem BuildMenuListViewItem(string icon, string text, StorageFolder folder)
        {
            ListViewItem lvi = new ListViewItem();
            lvi.Padding = new Thickness(0);
            ToolTipService.SetPlacement(lvi, PlacementMode.Mouse);
            ToolTipService.SetToolTip(lvi, folder.DisplayName);
            lvi.Tag = folder;
            StackPanel sp = new StackPanel();
            sp.Orientation = Orientation.Horizontal;
            sp.Tag = folder;
            lvi.Content = sp;
            Button btn = new Button();
            btn.FontFamily = new FontFamily("Segoe MDL2 Assets");
            btn.Content = icon;
            btn.Width = 48;
            btn.Height = 48;
            btn.Background = new SolidColorBrush(Colors.Transparent);
            btn.IsHitTestVisible = false;
            TextBlock tb = new TextBlock();
            tb.Text = text;
            tb.FontSize = 18;
            tb.VerticalAlignment = VerticalAlignment.Center;
            sp.Children.Add(btn);
            sp.Children.Add(tb);

            MenuFlyout itemFlyout = new MenuFlyout();
            MenuFlyoutItem removeItem = new MenuFlyoutItem();
            removeItem.Text = "Remove";
            removeItem.Click += MenuListViewItemRemove_Click;
            removeItem.Tag = lvi;
            itemFlyout.Items.Add(removeItem);
            lvi.ContextFlyout = itemFlyout;

            return lvi;
        }

        private void MenuListViewItemRemove_Click(object sender, RoutedEventArgs e)
        {
            MenuFlyoutItem source = e.OriginalSource as MenuFlyoutItem;
            ListViewItem lvi = source.Tag as ListViewItem;
            StorageFolder storageFolder = lvi.Tag as StorageFolder;
            AccessListEntry entry = StorageApplicationPermissions.FutureAccessList.Entries.ToList().Find(item => item.Metadata == storageFolder.Path);
            StorageApplicationPermissions.FutureAccessList.Remove(entry.Token);
            MenuListViewFolders.Items.Remove(lvi);
            Debug.WriteLine("Removed FolderItem with Name: " + entry.Metadata + " Token: " + entry.Token);
        }

        private async void MenuListViewFolders_Loaded(object sender, RoutedEventArgs e)
        {
            foreach (AccessListEntry entry in StorageApplicationPermissions.FutureAccessList.Entries.OrderBy(item => item.Metadata))
            {
                StorageFolder folder = await StorageFolder.GetFolderFromPathAsync(entry.Metadata);
                StorageApplicationPermissions.FutureAccessList.Add(folder, folder.Path);
                Debug.WriteLine("Opened the folder: " + folder.DisplayName);
                MenuListViewFolders.Items.Add(BuildMenuListViewItem("\xEDA2", folder.DisplayName, folder));
            }
        }

        private async void MenuListViewFolders_ItemClick(object sender, ItemClickEventArgs e)
        {
            StackPanel stackPanel = (StackPanel)e.ClickedItem;
            StorageFolder folder = (StorageFolder)stackPanel.Tag;
            Debug.WriteLine("[MenuListViewFolders_ItemClick] Clicked on: " + folder.DisplayName);
            FileListView.Items.Clear();
            IReadOnlyList<IStorageItem> folderItems = await folder.GetItemsAsync();
            foreach (IStorageItem folderItem in folderItems)
            {
                FileListView.Items.Add(BuildFileListViewItem(folderItem));
            }
            currentFolder = folder;
        }

        private async void FolderUpButton_Click(object sender, RoutedEventArgs e)
        {
            if (currentFolder != null)
            {
                StorageFolder parentFolder = await currentFolder.GetParentAsync();
                if (parentFolder != null)
                {
                    FileListView.Items.Clear();
                    IReadOnlyList<IStorageItem> folderItems = await parentFolder.GetItemsAsync();
                    foreach (IStorageItem folderItem in folderItems)
                    {
                        FileListView.Items.Add(BuildFileListViewItem(folderItem));
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
