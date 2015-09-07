using System.Windows.Media;

namespace Visualization
{
    class LineStyle
    {
        public DoubleCollection Dashes;
        public double Thickness;
        public Brush Stroke;

        public LineStyle(double thickness, Brush stroke, double[] dashes = null)
        {
            Thickness = thickness;
            Stroke = stroke;

            if (dashes != null)
                Dashes = new DoubleCollection(dashes);
        }
    }
}
