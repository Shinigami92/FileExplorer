using FileExplorer.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.AccessCache;
using Windows.Storage.Pickers;
using Windows.System;
using Windows.UI;
using Windows.UI.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace FileExplorer
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private StorageFolder currentFolder = null;
        private ObservableCollection<MenuFolderItem> MenuFolderItems = new ObservableCollection<MenuFolderItem>();
        private ObservableCollection<FileItem> FileItems = new ObservableCollection<FileItem>();

        public MainPage()
        {
            this.InitializeComponent();
            StorageApplicationPermissions.FutureAccessList.Entries.ToList().ForEach(e => Debug.WriteLine($"Metadata: {e.Metadata} Token: {e.Token}"));
            //StorageApplicationPermissions.FutureAccessList.Clear();
        }

        private void MenuButtonMainLeft_Click(object sender, RoutedEventArgs e)
        {
            MenuSplitViewMainLeft.IsPaneOpen = !MenuSplitViewMainLeft.IsPaneOpen;
        }

        private async void FileListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var fi = e.ClickedItem as FileItem;
            var storageItem = fi.StorageItem;
            Debug.WriteLine($"Clicked on: {fi.Name}");
            Debug.WriteLine(fi.ToolTipText);
            if (fi.IsFolder)
            {
                await NavigateToFolder(storageItem as StorageFolder);
            }
        }

        private async void MenuButtonMainAddFolder_ItemClick(object sender, ItemClickEventArgs e)
        {
            var folderPicker = new FolderPicker();
            folderPicker.FileTypeFilter.Add("*");
            folderPicker.SuggestedStartLocation = PickerLocationId.ComputerFolder;
            folderPicker.ViewMode = PickerViewMode.List;
            var folder = await folderPicker.PickSingleFolderAsync();
            if (folder != null)
            {
                StorageApplicationPermissions.FutureAccessList.Add(folder, folder.Path);
                Debug.WriteLine($"Opened the folder: {folder.DisplayName}");
                MenuFolderItems.Add(new MenuFolderItem(folder));
            }
            MenuButtonMainAddFolder.SelectedIndex = -1;
        }

        private void MenuListViewItemRemove_Click(object sender, RoutedEventArgs e)
        {
            var source = e.OriginalSource as MenuFlyoutItem;
            Debug.WriteLine($"source.Tag: {source.Tag}");
            var storageFolder = source.Tag as StorageFolder;
            var f = MenuFolderItems.ToList().Find(item => item.Folder == storageFolder);
            var entry = StorageApplicationPermissions.FutureAccessList.Entries.ToList().Find(item => item.Metadata == f.Folder.Path);
            StorageApplicationPermissions.FutureAccessList.Remove(entry.Token);
            MenuFolderItems.Remove(f);
            Debug.WriteLine($"Removed FolderItem with Name: {entry.Metadata} Token: {entry.Token}");
        }

        private async void MenuListViewFolders_Loaded(object sender, RoutedEventArgs e)
        {
            foreach (var entry in StorageApplicationPermissions.FutureAccessList.Entries.OrderBy(item => item.Metadata))
            {
                var folder = await StorageFolder.GetFolderFromPathAsync(entry.Metadata);
                StorageApplicationPermissions.FutureAccessList.Add(folder, folder.Path);
                Debug.WriteLine($"Opened the folder: {folder.DisplayName}");
                MenuFolderItems.Add(new MenuFolderItem(folder));
            }
        }

        private async void MenuListViewFolders_ItemClick(object sender, ItemClickEventArgs e)
        {
            var f = e.ClickedItem as MenuFolderItem;
            Debug.WriteLine($"Clicked on: {f.DisplayName}");
            await NavigateToFolder(f.Folder);
        }

        private async void FolderUpButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.currentFolder != null)
            {
                var parentFolder = await this.currentFolder.GetParentAsync();
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

                var folder = this.currentFolder;
                var parts = new List<StorageFolder> { folder };

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
            var btn = new Button
            {
                Content = folder.Name.TrimEnd('\\'),
                Tag = folder,
                FontSize = 20,
                FontWeight = FontWeights.SemiBold,
                Background = new SolidColorBrush(Colors.Transparent),
                VerticalAlignment = VerticalAlignment.Stretch,
                BorderThickness = new Thickness()
            };
            btn.Click += NavigateTo_Click;
            return btn;
        }

        private async void NavigateTo_Click(object sender, RoutedEventArgs e)
        {
            await NavigateToFolder((e.OriginalSource as Button).Tag as StorageFolder);
        }

        private TextBlock BuildCurrentFolderPathSeperator()
        {
            var tb = new TextBlock
            {
                FontFamily = new FontFamily("Segoe MDL2 Assets"),
                Text = "\xE937",
                FontSize = 12,
                Padding = new Thickness(4, 4, 0, 0),
                VerticalAlignment = VerticalAlignment.Center
            };
            return tb;
        }

        private void CommandBar_Opening(object sender, object e)
        {
            FolderUpButton.LabelPosition = CommandBarLabelPosition.Default;
        }

        private void CommandBar_Closing(object sender, object e)
        {
            FolderUpButton.LabelPosition = CommandBarLabelPosition.Collapsed;
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
                var folderItems = await folder.GetItemsAsync();
                foreach (var folderItem in folderItems)
                {
                    var fileItem = new FileItem(folderItem);
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

        private static readonly LauncherOptions OpenWithLaucherOptions = new LauncherOptions() { DisplayApplicationPicker = true };
        private async void FileItemOpen_Click(object sender, RoutedEventArgs e)
        {
            var source = e.OriginalSource as MenuFlyoutItem;
            var storageItem = source.Tag as IStorageItem;
            Debug.WriteLine($"storageItem = {storageItem}");
            if (storageItem.IsOfType(StorageItemTypes.File))
            {
                var storageFile = storageItem as StorageFile;
                bool success = await Launcher.LaunchFileAsync(storageFile);
                if (!success)
                {
                    success = await Launcher.LaunchFileAsync(storageFile, OpenWithLaucherOptions);
                }
                Debug.WriteLineIf(!success, "Launching the file failed");
            }
            else
            {
                await NavigateToFolder(storageItem as StorageFolder);
            }
        }
    }
}
