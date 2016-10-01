using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace FileExplorer.Models
{
    public class VisibleOnNullConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value != null)
            {
                if (value.GetType() == typeof(bool) && (bool)value == false)
                {
                    return Visibility.Visible;
                }
                return Visibility.Collapsed;
            }
            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
