using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskPlanning.Queueing
{
    public class OperationQueue
    {
        public ReadOnlyCollection<QueueItem> Items { get; internal set; }
        public string[] ParameterNames { get; internal set; }
        public string AlgorithmDescription { get; internal set; }
    }
}
