using GraphLogic.Entities;
using GraphLogic.Graphs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskPlanning.JobAssignment.Simulation;
using TaskPlanning.JobAssignment.Simulation.Helpers;
using TaskPlanning.JobAssignment.Simulation.Events;
using TaskPlanning.Queueing.Algorithms;

namespace TaskPlanning.JobAssignment.Algorithms
{
    public sealed class FirstFreeLongestIdleTime : JobAssignmentAlgorithm
    {
        protected override bool PeekBestFreeProcessor(Operation operation, out Processor result)
        {
            result = processors.Where(proc => schedule.IsProcessorFree(proc, currentTime, options.IoControllerPresent))
                               .OrderByDescending(proc => schedule.GetProcessorIdleTime(proc, currentTime, options.IoControllerPresent))
                               .FirstOrDefault();
            return (result != null);
        }

        public override int AlgorithmID
        {
            get { return 2; }
        }
    }
}
