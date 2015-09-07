using QuickGraph;
using QuickGraph.Algorithms;

using GraphLogic.Entities;
using GraphLogic.Graphs;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Threading.Tasks;

using GraphSharp.Algorithms.Layout.Simple.Hierarchical;
using GraphSharp.Algorithms.Layout.Simple.Tree;

namespace GraphLogic.Layouts
{
    using BallonTree = BalloonTreeLayoutAlgorithm<EditableValueBase, WeightedEdge, IBidirectionalGraph<EditableValueBase, WeightedEdge>>;
    using SimpleTree = SimpleTreeLayoutAlgorithm<EditableValueBase, WeightedEdge, IBidirectionalGraph<EditableValueBase, WeightedEdge>>;
    using EfficientSugiyama = EfficientSugiyamaLayoutAlgorithm<EditableValueBase, WeightedEdge, IVertexAndEdgeListGraph<EditableValueBase, WeightedEdge>>;

    public class TreeLayout : IGraphLayout
    {
        public double VerticalGap { get; set; }
        public double HorizontalGap { get; set; }

        public TreeLayout()
        {
            VerticalGap = 1.2;
            HorizontalGap = 0.9;
        }

        public IDictionary<EditableValueBase, Point> Compute(IWeightedGraph graph, IDictionary<EditableValueBase, Size> vertexSizes)
        {
            var directed = graph as IBidirectionalGraph<EditableValueBase, WeightedEdge>;
            var undirected = graph as IUndirectedGraph<EditableValueBase, WeightedEdge>;

            if (graph.IsDirected)
            {
                var parameters = new SimpleTreeLayoutParameters
                {
                    LayerGap = VerticalGap * vertexSizes.Values.Average(size => size.Height),
                    VertexGap = HorizontalGap * vertexSizes.Values.Average(size => size.Width)
                };

                var algo = new SimpleTree(directed, null, vertexSizes, parameters);
                algo.Compute();

                return algo.VertexPositions;
            }
            else
            {
                throw new NotSupportedException("Undirected graph not supported");
            }
        }
    }
}
