using GraphLogic.Entities;
using GraphLogic.Graphs;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace GraphLogic.Random
{
    public static class RandomGraph
    {
        public static DirectedWeightedGraph GenerateTask(TaskGenerationInfo info)
        {
            var newGraph = new DirectedWeightedGraph();

            var rand = new System.Random(Environment.TickCount);
            var vertices = new List<Operation>();

            for (int v = 1; v <= info.OperationCount; v++)
            {
                var vertex = new Operation(v)
                {
                    Type = OperationType.Function,
                    Complexity = rand.Next(info.MinComplexity, info.MaxComplexity + 1)
                };

                vertices.Add(vertex);
                newGraph.AddVertex(vertex);
            }

            var totalVertexComplexity = vertices.Sum(v => v.Complexity);
            var totalEdgeWeight = (int)(totalVertexComplexity / info.Connectivity - totalVertexComplexity);

            var minEdgeWeight = 1;
            var maxEdgeWeight = (int)Math.Ceiling(totalEdgeWeight / 8.0);

            if (maxEdgeWeight == 0)
                maxEdgeWeight = 2;

            var maxPossibleEdgeCount = vertices.Count * (vertices.Count - 1) / 2;
            var generatedEdges = new List<Tuple<int, int>>();

            while (totalEdgeWeight >= minEdgeWeight &&
                   generatedEdges.Count < maxPossibleEdgeCount)
            {
                /* Generate a random edge */
                var number = rand.Next(info.OperationCount * info.OperationCount);
                var start = number / info.OperationCount;
                var end = number % info.OperationCount;
                var edge = new Tuple<int, int>(start, end);

                /* Check if it's not a loop edge and if it isn't in the graph yet */
                if (start != end && !generatedEdges.Contains(edge))
                {
                    var weight = rand.Next(minEdgeWeight, maxEdgeWeight + 1);
                    if (weight > totalEdgeWeight)
                        weight = totalEdgeWeight;

                    var weightedEdge = new WeightedEdge(vertices[start], vertices[end], weight);
                    newGraph.AddEdge(weightedEdge);

                    /* Add the edge if the graph would remain acyclic */
                    if (newGraph.IsAcyclic)
                    {
                        generatedEdges.Add(edge);
                        totalEdgeWeight -= weight;
                    }
                    else
                    {
                        newGraph.RemoveEdge(weightedEdge);
                    }
                }
            }

            /* Distribute the remainder between the generated edges */
            var graphEdges = newGraph.Edges.ToArray();
            while (totalEdgeWeight > 0)
            {
                graphEdges[rand.Next(generatedEdges.Count)].Weight++;
                totalEdgeWeight--;
            }

            var coeff = newGraph.EdgeCount / maxPossibleEdgeCount;
            var comp = 1 - info.Connectivity;

            Debug.Assert(Math.Abs(coeff - comp) > 0.1);

            return newGraph;
        }

        public static DirectedWeightedGraph GenerateTask_Improved(TaskGenerationInfo info)
        {
            var newGraph = new DirectedWeightedGraph();

            var rand = new System.Random(Environment.TickCount);
            var vertices = new List<Operation>();

            for (int v = 1; v <= info.OperationCount; v++)
            {
                var vertex = new Operation(v)
                {
                    Type = OperationType.Function,
                    Complexity = rand.Next(info.MinComplexity, info.MaxComplexity + 1)
                };

                vertices.Add(vertex);
                newGraph.AddVertex(vertex);
            }

            var totalVertexComplexity = vertices.Sum(v => v.Complexity);
            var totalEdgeWeight = (int)(totalVertexComplexity / info.Connectivity - totalVertexComplexity);

            var minEdgeWeight = 1;
            var maxEdgeWeight = (int)Math.Ceiling(totalEdgeWeight / 8.0);

            if (maxEdgeWeight == 0)
                maxEdgeWeight = 2;

            var edgeCount = vertices.Count * (vertices.Count - 1) / 2;
            var generatedEdges = new List<Tuple<int, int>>();

            for (int e = 0; e < edgeCount; )
            {
                /* Generate a random edge */
                var number = rand.Next(info.OperationCount * info.OperationCount);
                var start = number / info.OperationCount;
                var end = number % info.OperationCount;
                var edge = new Tuple<int, int>(start, end);

                /* Check if it's not a loop edge and if it isn't in the graph yet */
                if (start != end && !generatedEdges.Contains(edge))
                {
                    var weight = rand.Next(minEdgeWeight, maxEdgeWeight + 1);
                    if (weight > totalEdgeWeight)
                        weight = totalEdgeWeight;

                    var weightedEdge = new WeightedEdge(vertices[start], vertices[end], weight);
                    newGraph.AddEdge(weightedEdge);

                    /* Add the edge if the graph would remain acyclic */
                    if (newGraph.IsAcyclic)
                    {
                        generatedEdges.Add(edge);
                        totalEdgeWeight -= weight;
                        e++;
                    }
                    else
                    {
                        newGraph.RemoveEdge(weightedEdge);
                    }
                }
            }

            /* Distribute the remainder between the generated edges */
            var graphEdges = newGraph.Edges.ToArray();
            while (totalEdgeWeight > 0)
            {
                graphEdges[rand.Next(generatedEdges.Count)].Weight++;
                totalEdgeWeight--;
            }

            return newGraph;
        }
    }
}
