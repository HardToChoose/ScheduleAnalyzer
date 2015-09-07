using GraphLogic.Entities;
using GraphLogic.Graphs;

using QuickGraph;

using System;
using System.Collections.Generic;
using System.Linq;

namespace TaskPlanning.Queueing.Statistics
{
    internal static class CriticalPathHelper
    {
        public static void GetLeafCriticalInfo(IEnumerable<Operation> leaves, DirectedWeightedGraph graph, IDictionary<EditableValueBase, VertexStat> result)
        {
            if (leaves.Count() == 0)
                return;

            foreach (var vertex in leaves)
            {
                result[vertex].LeafCriticalVertexPathLength += 1;
                result[vertex].LeafCriticalTimePathSum += vertex.Complexity;
            }
            var parents = leaves.SelectMany(vertex => graph.InEdges(vertex)
                                .Select(edge => edge.Source)
                                .Cast<Operation>())
                                .Distinct();

            foreach (var parent in parents)
            {
                result[parent].LeafCriticalVertexPathLength = graph.OutEdges(parent)
                                                       .Select(edge => result[edge.Target].LeafCriticalVertexPathLength)
                                                       .Max();
                result[parent].LeafCriticalTimePathSum = graph.OutEdges(parent)
                                                       .Select(edge => result[edge.Target].LeafCriticalTimePathSum)
                                                       .Max();
            }
            GetLeafCriticalInfo(parents, graph, result);
        }

        public static void GetRootCriticalInfo(IEnumerable<Operation> roots, DirectedWeightedGraph graph, IDictionary<EditableValueBase, VertexStat> result)
        {
            if (roots.Count() == 0)
                return;

            var children = roots.SelectMany(vertex => graph.OutEdges(vertex))
                                .Select(edge => edge.Target)
                                .Cast<Operation>()
                                .Distinct();

            foreach (var child in children)
            {
                result[child].RootCriticalVertexPathLength = graph.InEdges(child)
                                                      .Select(edge => result[edge.Source].RootCriticalVertexPathLength + 1)
                                                      .Max();
                result[child].RootCriticalTimePathSum = graph.InEdges(child)
                                                      .Select(edge => result[edge.Source].RootCriticalTimePathSum +
                                                                      (edge.Source as Operation).Complexity)
                                                      .Max();
            }
            GetRootCriticalInfo(children, graph, result);
        }

        public static IEnumerable<Operation> GetLeafCriticalVertexPath(DirectedWeightedGraph graph, IDictionary<EditableValueBase, VertexStat> info)
        {
            var path = new List<Operation>();
            var start = info.OrderByDescending(pair => pair.Value.LeafCriticalVertexPathLength).First().Key;

            Action<EditableValueBase> explorePath = null;
            explorePath = (from) =>
            {
                path.Add(from as Operation);
                var children = graph.OutEdges(from).Select(edge => edge.Target);

                if (children != null && children.Count() > 0)
                {
                    explorePath(children.OrderByDescending(child => info[child].LeafCriticalVertexPathLength).First());
                }
            };

            explorePath(start);
            return path;
        }
    }
}
