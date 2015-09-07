#if (DEBUG)

using GraphLogic.Entities;
using GraphLogic.Graphs;

using QuickGraph;
using QuickGraph.Serialization;

using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml;

using UI.Visualization;

namespace TaskPlanning.JobAssignment.Algorithms
{
    public partial class JobAssignmentAlgorithm
    {
        private void SavePcs(string id)
        {
            var vertexIdentity = new VertexIdentity<EditableValueBase>(proc => string.Format("{0}:{1}", proc.ID, (proc as Processor).Performance));
            var edgeIdentity = new EdgeIdentity<EditableValueBase, WeightedEdge>(edge => string.Format("{0}_{1}:{2}", edge.Source.ID, edge.Target.ID, edge.Weight));

            using (var writer = XmlWriter.Create(id + "_pcs.graphml"))
            {
                pcs.SerializeToGraphML(writer, vertexIdentity, edgeIdentity);
            }
        }

        private void SaveTask(string id)
        {
            var vertexIdentity = new VertexIdentity<EditableValueBase>(job => string.Format("{0}:{1}", job.ID, (job as Operation).Complexity));
            var edgeIdentity = new EdgeIdentity<EditableValueBase, WeightedEdge>(edge => string.Format("{0}_{1}:{2}", edge.Source.ID, edge.Target.ID, edge.Weight));

            using (var writer = XmlWriter.Create(id + "_task.graphml"))
            {
                task.SerializeToGraphML(writer, vertexIdentity, edgeIdentity);
            }
        }

        private void SaveSchedule(string id)
        {
            var canvas = new Canvas();
            var window = new Window { Content = canvas };

            window.Loaded += delegate
            {
                new GanntChartDrawer().Paint(schedule, options, canvas);
                CreateBitmapFromVisual(canvas, id + ".png");
                window.Close();
            };

            window.Show();
        }

        private static void CreateBitmapFromVisual(Visual target, string filename)
        {
            var bounds = VisualTreeHelper.GetDescendantBounds(target);
            var drawing = new DrawingVisual();

            using (var context = drawing.RenderOpen())
            {
                var brush = new VisualBrush(target);
                context.DrawRectangle(brush, null, new Rect(new Point(), bounds.Size));
            }

            var targetBitmap = new RenderTargetBitmap((int)bounds.Width, (int)bounds.Height, 96, 96, PixelFormats.Pbgra32);
            targetBitmap.Render(drawing);

            using (var stream = File.Create(filename))
            {
                var png = new PngBitmapEncoder();
                png.Frames.Add(BitmapFrame.Create(targetBitmap));
                png.Save(stream);
            }
        }
    }
}

#endif