using System.Collections.Generic;

namespace MainApp.ViewModels.AlgorithmComparison
{
    internal class Table
    {
        public string Title { get; set; }
        public List<int> JobCounts { get; set; }
        public IEnumerable<object> Data { get; set; }
    }
}
