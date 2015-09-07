using GraphLogic.Graphs;
using GraphLogic.Random;
using GraphLogic.Entities;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Threading.Tasks;

using TaskPlanning.Queueing.Algorithms;
using TaskPlanning.JobAssignment.Algorithms;
using TaskPlanning.Queueing.Statistics;
using TaskPlanning.JobAssignment;

namespace Measuring
{
    public class TestBench
    {
        private IntegerRange complexity;

        private double[] connectivity;
        private int[] jobCount;

        private long[,] taskGraphSingleProcessorExecutionTimes;
        private DirectedWeightedGraph[,] taskGraphs;
        private TaskGraphStat[,] taskGraphStats;

        private UndirectedWeightedGraph processorGraph;
        private int runs;

        public void GenerateTaskGraphs()
        {
            taskGraphs = new DirectedWeightedGraph[jobCount.Length * connectivity.Length, runs];
            taskGraphStats = new TaskGraphStat[taskGraphs.Length, runs];
            taskGraphSingleProcessorExecutionTimes = new long[taskGraphs.Length, runs];

            var info = new TaskGenerationInfo
            {
                MinComplexity = complexity.From,
                MaxComplexity = complexity.To
            };

            var index = 0;
            foreach (var jobs in jobCount)
            {
                foreach (var conn in connectivity)
                {
                    info.Connectivity = conn;
                    info.OperationCount = jobs;

                    for (int k = 0; k < runs; k++)
                    {
                        var graph = taskGraphs[index, k] = RandomGraph.GenerateTask(info);

                        taskGraphStats[index, k] = TaskGraphStat.Gather(graph);
                        taskGraphSingleProcessorExecutionTimes[index, k] = graph.Vertices.Sum(vertex => (vertex as Operation).Complexity);
#if (DEBUG)
                        var vertexSum = graph.Vertices.Cast<Operation>().Sum(op => op.Complexity);
                        var edgeSum = graph.Edges.Sum(edge => edge.Weight);

                        var realConn = vertexSum / (edgeSum + vertexSum);
                        var diff = realConn - conn;

                        Debug.Assert(Math.Abs(diff) < 0.1);
#endif
                    }
                    index++;
                }
            }
        }

        public TestBench(DoubleRange taskGraphConnectivity, IntegerRange taskGraphJobCount, IntegerRange jobComplexity, UndirectedWeightedGraph pcs, int testRuns)
        {
            processorGraph = pcs;
            runs = testRuns;

            connectivity = taskGraphConnectivity.ToArray();
            jobCount = taskGraphJobCount.ToArray();
            complexity = jobComplexity;

            GenerateTaskGraphs();
        }

        public TestResults Run(IQueueingAlgorithm queueMaker, JobAssignmentAlgorithm jobAssigner, PcsOptions options, IProgress<int> progress)
        {
            var timer = new Stopwatch();
            var result = new TestResults();
            
            for (int j = 0; j < jobCount.Length; j++)
            {
                var jobs = jobCount[j];

                result.SpeedUp[jobs] = new double[connectivity.Length];
                result.Efficiency[jobs] = new double[connectivity.Length];
                result.GanntTimes[jobs] = new double[connectivity.Length];

                result.AlgorithmExecutionTimes[jobs] = new double[connectivity.Length];

                for (int c = 0; c < connectivity.Length; c++)
                {
                    var totalSpeedUp = 0.0;
                    var totalEfficiency = 0.0;
                    var totalGanntTime = 0.0;
                    var totalAlgorithmExecutionTime = 0L;

                    int index = j * connectivity.Length + c;

                    /* Perform several test runs for a given pair of algorithms */
                    for (int k = 0; k < runs; k++)
                    {
                        timer.Restart();

                        var taskGraph = taskGraphs[index, k];
                        var queue = queueMaker.Compute(taskGraph.Vertices.Cast<Operation>(), taskGraphStats[index, k]);
                    
                        jobAssigner.Initialize(processorGraph, taskGraph, queue);
                        
                        var ganntTime = jobAssigner.Compute(options).Duration;
                        var speedUp = taskGraphSingleProcessorExecutionTimes[index, k] * 1.0 / ganntTime;
                        var efficiency = speedUp / jobs;

                        totalGanntTime += ganntTime;
                        totalEfficiency += efficiency;
                        totalSpeedUp += speedUp;

                        timer.Stop();

                        totalAlgorithmExecutionTime += timer.ElapsedMilliseconds;
                        progress.Report(index * runs + k);
                    }

                    /* Calculate mean execution time, speedup and efficiency coefficients */
                    result.SpeedUp[jobs][c] = totalSpeedUp / runs;
                    result.Efficiency[jobs][c] = totalEfficiency / runs;
                    result.GanntTimes[jobs][c] = totalGanntTime / runs;

                    result.AlgorithmExecutionTimes[jobs][c] = totalAlgorithmExecutionTime / runs;
                }
            }
            return result;
        }
    }
}
