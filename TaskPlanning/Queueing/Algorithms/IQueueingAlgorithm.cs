using GraphLogic.Entities;
using TaskPlanning.Queueing.Statistics;

using System.Collections.Generic;

namespace TaskPlanning.Queueing.Algorithms
{
    public interface IQueueingAlgorithm
    {
        string Name { get; }
        string Description { get; }

        OperationQueue Compute(IEnumerable<Operation> task, TaskGraphStat statistics);
    }
}
