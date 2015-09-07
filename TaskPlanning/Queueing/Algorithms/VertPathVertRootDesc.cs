using GraphLogic.Entities;
using TaskPlanning.Queueing.Statistics;

using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace TaskPlanning.Queueing.Algorithms
{
    public class VertPathVertRootDesc : IQueueingAlgorithm
    {
        public string Name
        {
            get { return "5"; }
        }

        public string Description
        {
            get { return "Спершу вершини з критичного шляху графа (по кількості вершин), а інші – в порядку спадання критичних шляхів до кінця графа за числом вершин."; }
        }

        public OperationQueue Compute(IEnumerable<Operation> task, TaskGraphStat statistics)
        {
            var result = new List<QueueItem>();

            foreach (var operation in statistics.MaxCriticalVertexPath)
            {
                result.Add(new QueueItem
                {
                    Value = operation,
                    Parameters = new [] { "так", "-" }
                });
            }

            result.AddRange(task.Except(statistics.MaxCriticalVertexPath)
                                .Select(operation => new QueueItem
                                {
                                    Value = operation,
                                    Parameters = new string[] { "-", statistics.Vertices[operation].LeafCriticalVertexPathLength.ToStr() }
                                })
                                .OrderByDescending(item => item.Parameters[1]));

            return new OperationQueue
            {
                Items = new ReadOnlyCollection<QueueItem>(result),
                ParameterNames = new [] { "Належить критичному шляху\nза числом вершин", "Кількість вершин\nдо кінця графу" },
                AlgorithmDescription = Description
            };
        }
    }
}
