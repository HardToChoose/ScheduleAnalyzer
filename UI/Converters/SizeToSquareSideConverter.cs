using System;
using System.Windows;
using System.Windows.Data;

namespace UI.Converters
{
    public class SizeToSquareSideConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            Size? size = value as Size?;
            return size.HasValue ? Math.Max(size.Value.Height, size.Value.Width) : 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
