using GraphLogic.Entities;
using QuickGraph;
using QuickGraph.Algorithms;
using QuickGraph.Serialization;

using System.ComponentModel;

namespace GraphLogic.Graphs
{
    public class DirectedWeightedGraph : BidirectionalGraph<EditableValueBase, WeightedEdge>, IWeightedGraph, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        protected void RaisePropertyChanged(string propertyName)
        {
            PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        public bool IsAcyclic
        {
            get
            {
                return this.IsDirectedAcyclicGraph<EditableValueBase, WeightedEdge>();
            }
        }

        public DirectedWeightedGraph()
        {
            EdgeAdded += GraphChanged;
            EdgeRemoved += GraphChanged;

            VertexAdded += GraphChanged;
            VertexRemoved += GraphChanged;

            Cleared += delegate { GraphChanged(null); };
        }

        private void GraphChanged(object arg)
        {
            RaisePropertyChanged("IsAcyclic");
        }
    }
}
