using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace FileExplorer.Converters
{
	public class VisibleOnExistsConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, string language)
		{
			if (value != null)
			{
				if (value.GetType() == typeof(bool) && (bool)value == false)
				{
					return Visibility.Collapsed;
				}
				return Visibility.Visible;
			}
			return Visibility.Collapsed;
		}

		public object ConvertBack(object value, Type targetType, object parameter, string language)
		{
			throw new NotImplementedException();
		}
	}
}
