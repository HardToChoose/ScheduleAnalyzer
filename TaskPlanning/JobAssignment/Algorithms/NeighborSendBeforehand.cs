using GraphLogic.Entities;

using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using TaskPlanning.JobAssignment.Simulation;
using TaskPlanning.JobAssignment.Simulation.Helpers;

namespace TaskPlanning.JobAssignment.Algorithms
{
    public sealed class NeighborSendBeforehand : JobAssignmentAlgorithm
    {
        protected override bool PeekBestFreeProcessor(Operation operation, out Processor result)
        {
            var inputData = task.InEdges(operation).ToArray();
            var freeProcessors = processors.Where(proc => schedule.IsProcessorFree(proc, currentTime, options.IoControllerPresent));

            var bestTransferTime = double.PositiveInfinity;
            result = null;

            foreach (var targetProc in freeProcessors)
            {
                var totalTransferTime = 0.0;

                /* Iterate over each data to be sent from parent jobs to the current one */
                foreach (var data in inputData)
                {
                    /* No data transfer needed for the same source and target processors */
                    var sourceProc = operationInfo[data.Source as Operation].AssignedProc;
                    if (sourceProc != targetProc)
                    {
                        totalTransferTime += routes.GetShortestRoutes(sourceProc, targetProc)
                                                   .First()
                                                   .Sum(channel => data.Weight / channel.TransferSpeed);
                    }
                }

                if (totalTransferTime < bestTransferTime)
                {
                    bestTransferTime = totalTransferTime;
                    result = targetProc;
                }
            }
            return (result != null);
        }

        public override int AlgorithmID
        {
            get { return 4; }
        }
    }
}
