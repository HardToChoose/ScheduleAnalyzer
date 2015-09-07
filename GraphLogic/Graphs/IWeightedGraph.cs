using GraphLogic.Entities;
using QuickGraph;

namespace GraphLogic.Graphs
{
    public interface IWeightedGraph :
        IGraph<EditableValueBase, WeightedEdge>,

        IMutableGraph<EditableValueBase, WeightedEdge>,
        IMutableEdgeListGraph<EditableValueBase, WeightedEdge>,

        IEdgeSet<EditableValueBase, WeightedEdge>,
        IVertexSet<EditableValueBase>,

        IMutableVertexSet<EditableValueBase>,
        IMutableVertexAndEdgeSet<EditableValueBase, WeightedEdge>
    {
    }
}
