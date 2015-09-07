using System.Collections.Generic;
using System.Linq;

namespace MainApp.ViewModels.AlgorithmComparison
{
    internal class TableRow
    {
        public double[] Items { get; set; }

        public static TableRow Create(Dictionary<int, double[]> dict, IEnumerable<int> columns, int rowIndex)
        {
            return new TableRow
            {
                Items = columns.Select(jobCount => dict[jobCount][rowIndex]).ToArray()
            };
        }
    }
}
