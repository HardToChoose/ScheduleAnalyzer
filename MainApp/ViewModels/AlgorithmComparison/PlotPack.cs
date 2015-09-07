using OxyPlot.Series;
using System.Collections.Generic;

namespace MainApp.ViewModels.AlgorithmComparison
{
    /// <summary>
    /// Job count to plot mappings
    /// </summary>
    internal class PlotPack
    {
        public Dictionary<int, Series> AlgorithmExecutionTimes { get; set; }

        public Dictionary<int, Series> GanntTimes { get; set; }
        public Dictionary<int, Series> Efficiency { get; set; }
        public Dictionary<int, Series> SpeedUp { get; set; }
    }
}
