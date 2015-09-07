using System;
using OxyPlot.Series;

namespace MainApp.ViewModels.AlgorithmComparison
{
    internal class DataSelector
    {
        public string Name { get; set; }
        public Func<PlotsAndTables, Table> SelectTable { get; set; }
        public Func<PlotsAndTables, int, Series> SelectPlot { get; set; }
    }
}
