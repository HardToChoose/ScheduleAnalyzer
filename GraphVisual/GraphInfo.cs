using GraphLogic.Layouts;
using System;
using System.Windows.Media;

namespace GraphVisual
{
    public class GraphInfo
    {
        public Type VertexDataType { get; set; }

        public ImageSource LayoutIcon { get; set; }
        public IGraphLayout Layout { get; set; }

        public string FileExtensions { get; set; }
    }
}
