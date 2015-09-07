using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using QuickGraph;
using QuickGraph.Algorithms;
using QuickGraph.Algorithms.Search;
using QuickGraph.Serialization;

using System.Runtime.Serialization;
using GraphLogic.Entities;

namespace GraphLogic.Graphs
{
    public class UndirectedWeightedGraph : UndirectedGraph<EditableValueBase, WeightedEdge>, IWeightedGraph, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        protected void RaisePropertyChanged(string propertyName)
        {
            PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        public bool IsConnected
        {
            get
            {
                return this.ConnectedComponents<EditableValueBase, WeightedEdge>(new Dictionary<EditableValueBase, int>()) == 1;
            }
        }

        public UndirectedWeightedGraph()
        {
            EdgeAdded += GraphChanged;
            EdgeRemoved += GraphChanged;

            VertexAdded += GraphChanged;
            VertexRemoved += GraphChanged;

            Cleared += delegate { GraphChanged(null); };
        }

        private void GraphChanged(object arg)
        {
            RaisePropertyChanged("IsConnected");
        }
    }
}
