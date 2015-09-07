using GraphLogic.Entities;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Data;

namespace UI.Converters
{
    public class OperationEnumerableToString : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var operations = value as IEnumerable<Operation>;
            if (operations == null)
                return null;

            return "{ " + string.Join(", ", operations.Select(o => o.ID)) + " }";
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
