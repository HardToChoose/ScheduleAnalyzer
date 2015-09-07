using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QuickGraph;
using GraphLogic.Graphs;
using GraphLogic.Entities;
using System.Windows;

namespace GraphLogic.Layouts
{
    public interface IGraphLayout
    {
        IDictionary<EditableValueBase, Point> Compute(IWeightedGraph graph, IDictionary<EditableValueBase, Size> vertexSizes);
    }
}