using QuickGraph;
using QuickGraph.Algorithms;

using GraphLogic.Entities;
using GraphLogic.Graphs;

using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TaskPlanning.JobAssignment.Simulation.Events;

namespace TaskPlanning.JobAssignment.Simulation.Helpers
{
    class FastestRouteFinder
    {
        private RouteTable routes;
        private PcsOptions options;

        private struct TransferInfo
        {
            public int SourceChannel;
            public int TargetCahnnel;

            public double StartTime;

            public TransferInfo(int source, int target, double time)
            {
                SourceChannel = source;
                TargetCahnnel = target;
                StartTime = time;
            }
        }

        private IReadOnlyList<Period>[] GetChannelFreePeriods(Schedule schedule, Processor proc, Processor source, Processor target, double timeNow)
        {
            var result = new IReadOnlyList<Period>[options.PhysicalLinks[proc]];

            /* Find all time periods when each source processor channel is free */
            for (int channelIndex = 0; channelIndex < result.Length; channelIndex++)
            {
                var channelTransfers = schedule[proc].OfType<DataTransfer>()
                                                     .Where(transfer => (transfer.UsesChannel(proc, channelIndex) || transfer.IsBetween(source, target))
                                                                        && !(options.DuplexTransfer && transfer.CanTransferDuplex(source, target)))
                                                                        
                                                     .ToArray();
                result[channelIndex] = FreeTimeFinder.GetFreePeriods(timeNow, channelTransfers);
            }
            return result;
        }

        private TransferInfo FindBestTransfer(Channel link, Schedule schedule, double transferDuration, double timeNow)
        {
            IReadOnlyList<Period> bothProcessorsFree = new List<Period> { Period.Whole };

            var source = link.Source;
            var target = link.Target;

            /* Find all time periods when source and target processors are free */
            if (options.IoControllerPresent == false)
            {
                bothProcessorsFree = FreeTimeFinder.FreePeriodsIntersection(
                    FreeTimeFinder.GetFreePeriods(timeNow, schedule[source]),
                    FreeTimeFinder.GetFreePeriods(timeNow, schedule[target]));
            }

            var sourceChannelFreePeriods = GetChannelFreePeriods(schedule, source, source, target, timeNow);
            var targetChannelFreePeriods = GetChannelFreePeriods(schedule, target, source, target, timeNow);

            var best = new TransferInfo(0, 0, double.PositiveInfinity);

            /* Find the time period of length 'transferDuration' which fits all free time period sequences */
            for (int a = 0; a < options.PhysicalLinks[source]; a++)
            {
                for (int b = 0; b < options.PhysicalLinks[target]; b++)
                {
                    var intersection = FreeTimeFinder.FreePeriodsIntersection(bothProcessorsFree,
                                        FreeTimeFinder.FreePeriodsIntersection(sourceChannelFreePeriods[a], targetChannelFreePeriods[b]));
                    var closestFit = intersection.FirstOrDefault(period => period.Duration >= transferDuration);

                    if (closestFit.From < best.StartTime)
                        best = new TransferInfo(a, b, closestFit.From);
                }
            }
            return best;
        }

        public FastestRouteFinder(RouteTable routes, PcsOptions options)
        {
            this.routes = routes;
            this.options = options;
        }

        public IEnumerable<Tuple<Channel, DataTransfer>> GetOptimalTransfers(Schedule schedule, Operation sourceOp, Operation targetOp, Processor source, Processor target, double startTime, double dataSize)
        {
            var minFinishTime = double.PositiveInfinity;
            var optimalTransfers = Enumerable.Empty<Tuple<Channel, DataTransfer>>();

            /* Simulate sending data over each shortest route */
            foreach (var path in routes.GetShortestRoutes(source, target))
            {
                /* Create a copy of the original schedule and simulation time counter */
                var tmp = schedule.Clone() as Schedule;
                var transfers = new List<Tuple<Channel, DataTransfer>>();
                var currentTime = startTime;

                foreach (var link in path)
                {
                    /* Find the appropriate time and channels to send data over current link */
                    var transferDuration = dataSize / link.TransferSpeed;
                    var info = FindBestTransfer(link, tmp, transferDuration, currentTime);

                    var transfer = new DataTransfer
                    {
                        DataSize = dataSize,

                        StartTime = info.StartTime,
                        Duration = transferDuration,

                        FromOperation = sourceOp,
                        FromProc = link.Source,
                        FromProcChannel = info.SourceChannel,
                        
                        ToOperation = targetOp,
                        ToProc = link.Target,
                        ToProcChannel = info.TargetCahnnel
                    };

                    transfers.Add(new Tuple<Channel, DataTransfer>(link, transfer));

                    /* Update the simulation schedule */
                    tmp.AddEvent(link.Source, transfer);
                    tmp.AddEvent(link.Target, transfer);

                    currentTime = info.StartTime + transferDuration;
                }

                /* Save the simulation for current path if it gives better time result */
                if (currentTime < minFinishTime)
                {
                    minFinishTime = currentTime;
                    optimalTransfers = transfers;
                }
            }
            return optimalTransfers;
        }
    }
}
