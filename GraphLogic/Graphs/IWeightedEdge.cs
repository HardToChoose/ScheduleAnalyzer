using QuickGraph;

namespace GraphLogic.Graphs
{
    public interface IWeightedEdge<V> : IEdge<V>
    {
        double Weight { get; set; }
    }
}
