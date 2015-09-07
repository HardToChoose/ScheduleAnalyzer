using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using TaskPlanning.JobAssignment;
using UI.Visualization;

namespace UI.Controls
{
    public partial class GanntChart : UserControl
    {
        private GanntChartDrawer drawer = new GanntChartDrawer();

        private static void ScheduleChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var @this = d as GanntChart;
            if (@this.Options != null && (e.NewValue as Schedule) != null)
            {
                @this.drawer.Paint(e.NewValue as Schedule, @this.Options, @this.Pane);
            }
        }

        public static readonly DependencyProperty OptionsProperty =
            DependencyProperty.Register("Options", typeof(PcsOptions), typeof(GanntChart), new PropertyMetadata(null));

        public static readonly DependencyProperty ScheduleProperty =
            DependencyProperty.Register("Schedule", typeof(Schedule), typeof(GanntChart), new PropertyMetadata(null, ScheduleChangedCallback));

        public PcsOptions Options
        {
            get { return (PcsOptions)GetValue(OptionsProperty); }
            set { SetValue(OptionsProperty, value); }
        }

        public Schedule Schedule
        {
            get { return (Schedule)GetValue(ScheduleProperty); }
            set { SetValue(ScheduleProperty, value); }
        }        

        public GanntChart()
        {
            InitializeComponent();
        }

        public void SaveToPng(string fileName)
        {
            CreateBitmapFromVisual(Pane, fileName);
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
