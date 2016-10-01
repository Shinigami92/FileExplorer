using System;
using Windows.Storage.FileProperties;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace FileExplorer.Models
{
    public class ThumbnailToImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            BitmapImage image = null;
            if (value != null)
            {
                if (value.GetType() != typeof(StorageItemThumbnail))
                {
                    throw new ArgumentException("Expected a thumbnail");
                }
                else if (targetType != typeof(ImageSource))
                {
                    throw new ArgumentException("What are you trying to convert to here?");
                }
                StorageItemThumbnail thumbnail = value as StorageItemThumbnail;
                thumbnail.Seek(0);
                image = new BitmapImage();
                image.SetSource(thumbnail);
            }
            return image;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
