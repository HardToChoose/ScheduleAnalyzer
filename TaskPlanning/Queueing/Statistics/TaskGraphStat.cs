using GraphLogic.Entities;
using GraphLogic.Graphs;

using QuickGraph;
using QuickGraph.Algorithms;
using QuickGraph.Algorithms.RandomWalks;
using QuickGraph.Algorithms.Observers;
using QuickGraph.Algorithms.Search;

using System.Collections.Generic;
using System.Linq;
using System;

namespace TaskPlanning.Queueing.Statistics
{
    public class TaskGraphStat
    {
        public IReadOnlyDictionary<EditableValueBase, VertexStat> Vertices { get; internal set; }

        public IEnumerable<Operation> MaxCriticalVertexPath { get; internal set; }
        public IEnumerable<Operation> MaxCriticalTimePath { get; internal set; }

        public double MaxCriticalVertexPathLength { get; internal set; }
        public double MaxCriticalTimePathSum { get; internal set; }

        internal TaskGraphStat() { }

        private static IEnumerable<IEnumerable<Operation>> GetDisconnectedComponents(DirectedWeightedGraph graph)
        {
            var result = new List<IEnumerable<Operation>>();
            var algo = new BidirectionalDepthFirstSearchAlgorithm<EditableValueBase, WeightedEdge>(graph);

            var visited = new List<EditableValueBase>();
            var roots = graph.Roots();

            foreach (var root in roots)
            {
                if (!visited.Contains(root))
                {
                    var component = new List<EditableValueBase>();
                    VertexAction<EditableValueBase> observer = v => component.Add(v);

                    algo.DiscoverVertex += observer;
                    algo.Compute(roots.First());
                    algo.DiscoverVertex -= observer;

                    visited.AddRange(component);
                    result.Add(component.Cast<Operation>());
                }
            }
            return result;
        }

        public static TaskGraphStat Gather(DirectedWeightedGraph graph)
        {
            var vertexStats = new Dictionary<EditableValueBase, VertexStat>();
            foreach (var vertex in graph.Vertices)
            {
                vertexStats[vertex] = new VertexStat();
            }

            var components = new Dictionary<EditableValueBase, int>();
            graph.WeaklyConnectedComponents(components);

            foreach (var subgraph in graph.Vertices.GroupBy(vertex => components[vertex]))
            {
                var roots = subgraph.Where(vertex => graph.InDegree(vertex) == 0);
                var leaves = subgraph.Where(vertex => graph.OutDegree(vertex) == 0);

                CriticalPathHelper.GetRootCriticalInfo(roots.Cast<Operation>(), graph, vertexStats);
                CriticalPathHelper.GetLeafCriticalInfo(leaves.Cast<Operation>(), graph, vertexStats);
            }

            return new TaskGraphStat
            {
                Vertices = vertexStats,

                MaxCriticalVertexPath = CriticalPathHelper.GetLeafCriticalVertexPath(graph, vertexStats),

                MaxCriticalVertexPathLength = vertexStats.Values.Max(v => v.LeafCriticalVertexPathLength),
                MaxCriticalTimePathSum = vertexStats.Values.Max(v => v.LeafCriticalTimePathSum)
            };
        }
    }
}
