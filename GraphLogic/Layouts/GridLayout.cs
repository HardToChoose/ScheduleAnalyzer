using GraphLogic.Entities;
using GraphLogic.Graphs;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace GraphLogic.Layouts
{
    public class GridLayout : IGraphLayout
    {
        public IDictionary<EditableValueBase, Point> Compute(IWeightedGraph graph, IDictionary<EditableValueBase, Size> vertexSizes)
        {
            return null;
        }
    }
}
