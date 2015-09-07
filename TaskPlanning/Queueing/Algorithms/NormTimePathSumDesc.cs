using Entities;
using GraphLogic.Entities;
using TaskPlanning.Queueing.Statistics;

using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace TaskPlanning.Queueing.Algorithms
{
    public class NormTimeVertSumDesc : IQueueingAlgorithm
    {
        public string Name
        {
            get { return "1"; }
        }

        public string Description
        {
            get { return "За спаданням суми пронормованих критичних шляхів до кінця графу задачі по часу та кількості вершин"; }
        }

        public OperationQueue Compute(IEnumerable<Operation> task, TaskGraphStat statistics)
        {
            return new OperationQueue
            {
                Items = task.Select(operation => new QueueItem
                                                 {
                                                     Value = operation,
                                                     Parameters = new string[]
                                                     {
                                                         (statistics.Vertices[operation].LeafCriticalVertexPathLength / statistics.MaxCriticalVertexPathLength +
                                                         statistics.Vertices[operation].LeafCriticalTimePathSum / statistics.MaxCriticalTimePathSum).ToStr()
                                                     }
                                                 })
                            .OrderByDescending(item => item.Parameters[0])
                            .ToReadOnlyCollection(),
                ParameterNames = new [] { "Сума пронормованих\nкритичних шляхів" },
                AlgorithmDescription = Description
            };
        }
    }
}
