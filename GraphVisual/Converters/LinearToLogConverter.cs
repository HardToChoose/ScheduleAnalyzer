using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace GraphVisual.Converters
{
    public class LinearToLogConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (values.Length != 3)
            {
                return 0;
            }

            var min = (double)values[0];
            var max = (double)values[1];
            var value = (double)values[2];

            if (min < 0 || max < 0)
                throw new InvalidOperationException();

            var middle = (max + min) / 2;
            return 1 + (value - middle) / middle;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
