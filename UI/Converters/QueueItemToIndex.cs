using System;
using System.Windows.Data;

using TaskPlanning.Queueing;

namespace UI.Converters
{
    public class QueueItemToIndex : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var item = value as QueueItem;
            var queue = parameter as OperationQueue;

            if (item == null || queue == null)
                return -1;
            
            return queue.Items.IndexOf(item) + 1;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
