using GraphLogic.Entities;
using GraphLogic.Graphs;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace TaskPlanning.JobAssignment.Simulation.Helpers
{
    internal class RouteTable
    {
        private UndirectedWeightedGraph pcs;
        private EditableValueBase[] vertices;

        private Dictionary<Processor, Dictionary<Processor, Channel[][]>> routes;

        private Channel[][] ReversePathes(Channel[][] pathes)
        {
            var result = new Channel[pathes.Length][];
            for (int k = 0; k < pathes.Length; k++)
            {
                result[k] = pathes[k].Reverse().Select(channel => channel.Reverse()).ToArray();
            }
            return result;
        }

        public RouteTable(UndirectedWeightedGraph pcs)
        {
            this.pcs = pcs;
            this.vertices = pcs.Vertices.ToArray();
            this.routes = new Dictionary<Processor, Dictionary<Processor, Channel[][]>>();

            foreach (var vertex in vertices)
            {
                routes[vertex as Processor] = new Dictionary<Processor, Channel[][]>();
            }

            for (int u = 0; u < pcs.VertexCount; u++)
            {
                var source = vertices[u] as Processor;

                for (int v = u + 1; v < pcs.VertexCount; v++)
                {
                    var dest = vertices[v] as Processor;

                    routes[source][dest] = ShortestPathes(u, v);
                    routes[dest][source] = ReversePathes(routes[source][dest]);
                }
            }
        }

        public Channel[][] GetShortestRoutes(Processor source, Processor target)
        {
            return routes[source][target];
        }

        private void Djkstra(int source, double[] dist, int[,] prev)
        {
            List<int> nonVisited = new List<int>();

            /* Mark all vertices as non-visited, set the distance to all vertices except the source to infinity */
            for (int v = 0; v < pcs.VertexCount; v++)
            {
                dist[v] = int.MaxValue;
                prev[v, 0] = 0;					// the number of alternative previous vertices
                nonVisited.Add(v);
            }
            dist[source] = 0;

            while (nonVisited.Count > 0)
            {
                /* Find and erase a non-visited vertex 'u' with a minimum distance */
                int u = nonVisited.OrderBy(v => dist[v]).First();
                nonVisited.Remove(u);

                /* For each vertex 'v' adjacent to vertex 'u' */
                foreach (var edge in pcs.AdjacentEdges(vertices[u]))
                {
                    int v = Array.IndexOf(vertices, (edge.Source == vertices[u]) ? edge.Target : edge.Source);
                    double newDist = dist[u] + 1 / edge.Weight;

                    /* Make 'u' an alternative previous vertex for 'v' */
                    if (newDist == dist[v])
                    {
                        prev[v, 0]++;
                        prev[v, prev[v, 0]] = u;
                    }

                    /* Update the shortest distance to 'v' */
                    else if (newDist < dist[v])
                    {
                        dist[v] = newDist;
                        prev[v, 0] = 1;
                        prev[v, 1] = u;
                    }
                }
            }
        }

        /// <summary
        /// Finds all shortest paths to 'dest' vertex recursively
        /// </summary>
        private void ConstructPathes(List<int> initial, int source, int dest, int[,] prev, List<Channel[]> pathes)
        {
            while (prev[dest, 0] != 0)
            {
                int alternatives = prev[dest, 0];
                initial.Insert(0, dest);

                /* Check if there's only one alternative path through current vertex */
                if (alternatives == 1)
                {
                    dest = prev[dest, 1];
                }
                else
                {
                    for (int k = 1; k <= alternatives; k++)
                    {
                        List<int> newPath = new List<int>(initial);
                        ConstructPathes(newPath, source, prev[dest, k], prev, pathes);
                    }
                    return;
                }
            }

            var path = new List<Channel>();
            initial.Insert(0, source);

            /* Convert the sequence of adjacent vertices to the list of channels */
            for (int k = 0; k < initial.Count - 1; k++)
            {
                var from = vertices[initial[k]] as Processor;
                var to = vertices[initial[k + 1]] as Processor;
                
                WeightedEdge edge;
                pcs.TryGetEdge(from, to, out edge);

                path.Add(new Channel
                {
                    Source = from,
                    Target = to,
                    TransferSpeed = edge.Weight
                });
            }
            pathes.Add(path.ToArray());
        }

        /// <summary>
        /// Finds all shortest paths between 'source' and 'dest' processors
        /// </summary>
        private Channel[][] ShortestPathes(int source, int dest)
        {
            /* Allocate memory for distance and previous vertex arrays */
            double[] dist = new double[pcs.VertexCount];
            int[,] prev = new int[pcs.VertexCount, pcs.VertexCount + 1];

            /* Find the shortest paths from 'source' to all other intersections */
            Djkstra(source, dist, prev);
            double time = dist[dest];

            /* Construct all shortest paths from 'source' to 'dest' */
            var main = new List<int>();
            var pathes = new List<Channel[]>();

            ConstructPathes(main, source, dest, prev, pathes);
            return pathes.ToArray();
        }
    }
}
