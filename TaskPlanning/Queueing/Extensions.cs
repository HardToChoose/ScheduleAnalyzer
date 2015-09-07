using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;

namespace TaskPlanning.Queueing
{
    internal static class Extensions
    {
        public static string ToStr(this double number)
        {
            return number.ToString("0.##", CultureInfo.InvariantCulture);
        }

        public static ReadOnlyCollection<T> ToReadOnlyCollection<T>(this IEnumerable<T> enumerable)
        {
            return new ReadOnlyCollection<T>(enumerable.ToArray());
        }
    }
}