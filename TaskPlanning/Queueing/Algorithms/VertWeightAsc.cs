using GraphLogic.Entities;
using TaskPlanning.Queueing.Statistics;

using System.Linq;
using System.Collections.Generic;
using System.Globalization;

namespace TaskPlanning.Queueing.Algorithms
{
    public class VertWeightAsc : IQueueingAlgorithm
    {
        public string Name
        {
            get { return "15"; }
        }

        public string Description
        {
            get { return "У порядку зростання ваги вершин"; }
        }

        public OperationQueue Compute(IEnumerable<Operation> task, TaskGraphStat statistics)
        {
            return new OperationQueue
            {
                Items = task.OrderBy(operation => operation.Complexity)
                            .Select(operation => new QueueItem
                                                 {
                                                     Value = operation,
                                                     Parameters = new string[] { operation.Complexity.ToString() }
                                                 })
                            .ToReadOnlyCollection(),
                ParameterNames = new [] { "Вага вершини" },
                AlgorithmDescription = Description
            };
        }
    }
}
