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
using Windows.UI.Text;
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
            Debug.WriteLine(fi.ToolTipText);
            if (fi.IsFolder)
            {
                await NavigateToFolder(storageItem as StorageFolder);
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
            MenuButtonMainAddFolder.SelectedIndex = -1;
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
            await NavigateToFolder(f.Folder);
        }

        private async void FolderUpButton_Click(object sender, RoutedEventArgs e)
        {
            if (currentFolder != null)
            {
                StorageFolder parentFolder = await currentFolder.GetParentAsync();
                await NavigateToFolder(parentFolder);
            }
            else
            {
                Debug.WriteLine("There was no currentFolder");
            }
        }

        private async void UpdateCurrentFolderPathPanel()
        {
            if (this.currentFolder != null)
            {
                CurrentFolderPathPanel.Children.Clear();

                StorageFolder folder = this.currentFolder;
                List<StorageFolder> parts = new List<StorageFolder>();
                parts.Add(folder);

                try
                {
                    while ((folder = await folder.GetParentAsync()) != null)
                    {
                        parts.Add(folder);
                    }
                }
                catch (Exception)
                {
                    Debug.WriteLine("You don't have access permissions to this parent folder!");
                }

                parts.Reverse();
                CurrentFolderPathPanel.Children.Add(BuildCurrentFolderPathButton(parts.ElementAt(0)));
                for (int i = 1; i < parts.Count; i++)
                {
                    CurrentFolderPathPanel.Children.Add(BuildCurrentFolderPathSeperator());
                    CurrentFolderPathPanel.Children.Add(BuildCurrentFolderPathButton(parts.ElementAt(i)));
                }
            }
        }

        private Button BuildCurrentFolderPathButton(StorageFolder folder)
        {
            Button btn = new Button();
            btn.Content = folder.Name.TrimEnd('\\');
            btn.Tag = folder;
            btn.FontSize = 20;
            btn.FontWeight = FontWeights.SemiBold;
            btn.Background = new SolidColorBrush(Colors.Transparent);
            btn.VerticalAlignment = VerticalAlignment.Stretch;
            btn.BorderThickness = new Thickness();
            btn.Click += NavigateTo_Click;
            return btn;
        }

        private async void NavigateTo_Click(object sender, RoutedEventArgs e)
        {
            await NavigateToFolder((e.OriginalSource as Button).Tag as StorageFolder);
        }

        private TextBlock BuildCurrentFolderPathSeperator()
        {
            TextBlock tb = new TextBlock();
            tb.FontFamily = new FontFamily("Segoe MDL2 Assets");
            tb.Text = "\xE937";
            tb.FontSize = 12;
            tb.Padding = new Thickness(4, 4, 0, 0);
            tb.VerticalAlignment = VerticalAlignment.Center;
            return tb;
        }

        private void CommandBar_Opening(object sender, object e)
        {
            FolderUpButton.Label = "Folder Up";
        }

        private void CommandBar_Closing(object sender, object e)
        {
            FolderUpButton.Label = "";
        }

        private void MenuSplitViewMainLeft_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (e.NewSize.Width <= 640)
            {
                MenuSplitViewMainLeft.DisplayMode = SplitViewDisplayMode.CompactOverlay;
            }
            else
            {
                MenuSplitViewMainLeft.DisplayMode = SplitViewDisplayMode.CompactInline;
            }
        }

        private async void RefreshFolderButton_Click(object sender, RoutedEventArgs e)
        {
            await NavigateToFolder(this.currentFolder);
        }

        private async Task NavigateToFolder(StorageFolder folder)
        {
            FileItems.Clear();
            if (folder != null)
            {
                IReadOnlyList<IStorageItem> folderItems = await folder.GetItemsAsync();
                foreach (IStorageItem folderItem in folderItems)
                {
                    FileItem fileItem = new FileItem(folderItem);
                    await fileItem.FetchProperties();
                    FileItems.Add(fileItem);
                }
            }
            else
            {
                Debug.WriteLine("folder was null");
            }
            this.currentFolder = folder;
            UpdateCurrentFolderPathPanel();
        }

        private void ToggleViewButton_Click(object sender, RoutedEventArgs e)
        {
            if (FileItemsListView.Visibility == Visibility.Visible)
            {
                ToggleViewButton.Icon = new SymbolIcon(Symbol.List);
                FileItemsListView.Visibility = Visibility.Collapsed;
                FileItemsGridView.Visibility = Visibility.Visible;
            }
            else
            {
                ToggleViewButton.Icon = new SymbolIcon(Symbol.ViewAll);
                FileItemsListView.Visibility = Visibility.Visible;
                FileItemsGridView.Visibility = Visibility.Collapsed;
            }
        }
    }
}
