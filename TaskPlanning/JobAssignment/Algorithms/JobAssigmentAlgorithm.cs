using GraphLogic.Entities;
using GraphLogic.Graphs;

using System;
using System.Linq;
using System.Collections.Generic;

using TaskPlanning.Queueing;
using TaskPlanning.Queueing.Algorithms;

using TaskPlanning.JobAssignment.Simulation;
using TaskPlanning.JobAssignment.Simulation.Events;
using TaskPlanning.JobAssignment.Simulation.Helpers;

namespace TaskPlanning.JobAssignment.Algorithms
{
    public abstract partial class JobAssignmentAlgorithm
    {
        protected List<Operation> operationQueue;
        protected IEnumerable<Processor> processors;
        protected Dictionary<Operation, OperationInfo> operationInfo;

        protected Schedule schedule;
        internal RouteTable routes;

        protected PcsOptions options;
        private FastestRouteFinder routeFinder;

        protected double currentTime;

        protected UndirectedWeightedGraph pcs;
        protected DirectedWeightedGraph task;

        public void Initialize(UndirectedWeightedGraph mppGraph, DirectedWeightedGraph taskGraph, OperationQueue queue)
        {
            operationQueue = new List<Operation>(queue.Items.Select(item => item.Value));
            operationInfo = new Dictionary<Operation, OperationInfo>();
            
            processors = mppGraph.Vertices.Cast<Processor>();
            routes = new RouteTable(mppGraph);

            pcs = mppGraph;
            task = taskGraph;
        }

        protected void JumpToNextFinish()
        {
            /* Find all operations to be finished in the closest future at the same time */
            var finished = operationInfo.Where(_ => _.Value.State == OperationState.Started)// &&
                                                    //_.Value.FinishTime >= currentTime)
                                        .GroupBy(_ => _.Value.FinishTime)
                                        .OrderBy(group => group.Key)
                                        .FirstOrDefault();
            var states = operationInfo.OrderBy(pair => pair.Key.ID).Select(op => string.Format("{0}: {1}", op.Key.ID, op.Value.State)).ToArray();
            if (finished == null)
            {
                currentTime = processors.Select(proc => schedule[proc].Last(period => !options.IoControllerPresent || (period is JobExecution)))
                                        .Min(period => period.FinishTime);
                return;
            }

            /* Mark them as finished */
            foreach (var pair in finished)
            {
                pair.Value.State = OperationState.Finished;
            }

            /* Check if there are new operations ready to execution */
            foreach (var op in operationQueue.Where(op => (operationInfo[op].State == OperationState.NotReady) && AllParentsFinished(op)))
            {
                operationInfo[op].State = OperationState.Ready;
            }
            currentTime = finished.Key;
        }

        private double PrepareDataNoBeforehand(Operation operation, Processor targetProc)
        {
            /* No data needed for the root nodes */
            var inputData = task.InEdges(operation).ToArray();
            if (inputData.Length == 0)
                return 0.0;

            var latestParentFinish = inputData.Select(edge => edge.Source as Operation)
                                              .Max(op => operationInfo[op].FinishTime);
            var latestSendFinish = latestParentFinish;

            /* Iterate over each data to be sent from parent jobs to the current one */
            foreach (var data in inputData.OrderBy(edge => edge.Weight))
            {
                /* No data transfer needed for the same source and target processors */
                var sourceProc = operationInfo[data.Source as Operation].AssignedProc;
                if (sourceProc == targetProc)
                    continue;

                foreach (var link_transfer in routeFinder.GetOptimalTransfers(
                    schedule,
                    sourceOp: data.Source as Operation,
                    targetOp: operation,

                    source: sourceProc,
                    target: targetProc,

                    startTime: latestParentFinish,
                    dataSize: data.Weight))
                {
                    schedule.AddEvent(link_transfer.Item1.Source, link_transfer.Item2);
                    schedule.AddEvent(link_transfer.Item1.Target, link_transfer.Item2);

                    latestSendFinish = Math.Max(latestSendFinish, link_transfer.Item2.FinishTime);
                }
            }
            return latestSendFinish;
        }

        protected void ScheduleOperation(Operation operation, Processor target, double dataReceivedTime)
        {
            /* Get all timeline parts when the target processor is busy */
            var consideredEvents = (options.IoControllerPresent ?
                schedule[target].Where(e => e is JobExecution) :
                schedule[target]).ToList();

            /* Find free timeline part with the needed duration which starts from t >= dataReceivedTime */
            double duration = operation.Complexity / target.Performance;
            double time = FreeTimeFinder.FindFirstFreePeriod(dataReceivedTime, duration, consideredEvents);

            operationInfo[operation].AssignedProc = target;
            operationInfo[operation].State = OperationState.Started;

            operationInfo[operation].StartTime = time;
            operationInfo[operation].FinishTime = time + duration;

            schedule.AddEvent(target, new JobExecution
            {
                Job = operation,
                StartTime = time,
                Duration = duration
            });
        }

        private bool PeekPreferredOperation(out Operation result)
        {
            /* Select the most prioritized operation which hasn't started yet but whose parents are completed */
            result = operationQueue.FirstOrDefault(op => operationInfo[op].State == OperationState.Ready);
            return (result != null);
        }

        public virtual Schedule Compute(PcsOptions options)
        {
            schedule = new Schedule(processors);
            currentTime = 0;

            foreach (var _ in operationQueue)
            {
                operationInfo[_] = new OperationInfo();
            }

            if (task.VertexCount == 0 || pcs.VertexCount == 0)
                return schedule;

            this.options = options;
            this.routeFinder = new FastestRouteFinder(routes, options);

            currentTime = 0;

            Processor proc;
            Operation op;

            foreach (var root in task.Vertices.Where(v => task.InDegree(v) == 0))
            {
                operationInfo[root as Operation].State = OperationState.Ready;
            }

            while (!AllFinished() && PeekPreferredOperation(out op) && PeekBestFreeProcessor(op, out proc))
            {
                ScheduleOperation(op, proc, PrepareDataNoBeforehand(op, proc));
            }

            while (!AllFinished())
            {
                JumpToNextFinish();
                while (PeekPreferredOperation(out op) && PeekBestFreeProcessor(op, out proc))
                {
                    ScheduleOperation(op, proc, PrepareDataNoBeforehand(op, proc));
                }
            }
            return schedule;
        }

        protected bool IsOperationFinished(Operation operation)
        {
            return (operationInfo[operation].State == OperationState.Finished);
        }

        protected bool IsOperationReady(Operation operation)
        {
            return (operationInfo[operation].State == OperationState.Ready);
        }

        protected double LastParentFinishTime(Operation operation)
        {
            return task.InEdges(operation)
                       .Max(edge => operationInfo[edge.Source as Operation].FinishTime);
        }

        protected bool AllParentsFinished(Operation operation)
        {
            return task.InEdges(operation)
                       .Select(edge => edge.Source as Operation)
                       .All(parent => IsOperationFinished(parent));
        }

        protected bool AllFinished()
        {
            return operationInfo.All(pair => pair.Value.State == OperationState.Finished);
        }

        public abstract int AlgorithmID { get; }

        protected abstract bool PeekBestFreeProcessor(Operation operation, out Processor result);
    }
}