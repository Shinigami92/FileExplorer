using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.AccessCache;
using Windows.Storage.Pickers;
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
        public MainPage()
        {
            this.InitializeComponent();

            StorageApplicationPermissions.FutureAccessList.Clear();

            ItemCollection fileListViewItems = FileListView.Items;
            System.Diagnostics.Debug.WriteLine("FileListViewItems.Count: " + fileListViewItems.Count);
            fileListViewItems.Clear();

            for (int i = 0; i < 100; i++)
            {
                fileListViewItems.Add(buildFileListViewItem("Column 01", "Column 02"));
            }
        }

        private ListViewItem buildFileListViewItem(string textLeft, string textRight)
        {
            Grid itemGrid = new Grid();
            itemGrid.ColumnDefinitions.Add(new ColumnDefinition());
            itemGrid.ColumnDefinitions.Add(new ColumnDefinition());
            TextBlock tbLeft = new TextBlock();
            tbLeft.Text = textLeft;
            Grid.SetColumn(tbLeft, 0);
            itemGrid.Children.Add(tbLeft);
            TextBlock tbRight = new TextBlock();
            tbRight.Text = textRight;
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

        private async void MenuButtonThisPC_Click(object sender, RoutedEventArgs e)
        {
            FolderPicker folderPicker = new FolderPicker();
            folderPicker.ViewMode = PickerViewMode.List;
            folderPicker.SuggestedStartLocation = PickerLocationId.ComputerFolder;
            folderPicker.FileTypeFilter.Add(".mp3");
            StorageFolder folder = await folderPicker.PickSingleFolderAsync();
            StorageApplicationPermissions.FutureAccessList.Entries.ToList().ForEach(el => System.Diagnostics.Debug.WriteLine("Token: " + el.Token));
            StorageApplicationPermissions.FutureAccessList.Add(folder, "c");
            System.Diagnostics.Debug.WriteLine("MaximumItemsAllowed: " + StorageApplicationPermissions.FutureAccessList.MaximumItemsAllowed);
            System.Diagnostics.Debug.WriteLine("Element: " + StorageApplicationPermissions.FutureAccessList.Entries);
            System.Diagnostics.Debug.WriteLine("Element: " + folder.Name);
            IReadOnlyList<IStorageItem> storageItems = await folder.GetItemsAsync();
            FileListView.Items.Clear();
            foreach (IStorageItem storageItem in storageItems)
            {
                System.Diagnostics.Debug.WriteLine("Element: " + storageItem.Name);
                FileListView.Items.Add(buildFileListViewItem(storageItem.Name, storageItem.DateCreated.ToString()));
            }
        }

        private async void MenuButtonLocalDiskC_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                StorageFolder folder = await StorageFolder.GetFolderFromPathAsync(@"C:\");
                System.Diagnostics.Debug.WriteLine("Element: " + folder.Name);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.Fail(ex.Message);
            }
        }

        private void FileListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("Clicked on: " + e.ClickedItem);
        }

        private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListViewItem lvi = ((ListViewItem)e.AddedItems.First());
            System.Diagnostics.Debug.WriteLine("Clicked on: " + lvi.Name);
        }
    }
}
