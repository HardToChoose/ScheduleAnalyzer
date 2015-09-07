using GraphLogic.Entities;

using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

using TaskPlanning.JobAssignment;
using TaskPlanning.JobAssignment.Simulation.Events;

namespace Visualization
{
    public class GanntChartDrawer
    {
        private static readonly LineStyle AxisStyle = new LineStyle(2, Brushes.Black);
        private static readonly LineStyle InnerStyle = new LineStyle(0.3, Brushes.Gray);
        private static readonly LineStyle FinishStyle = new LineStyle(1.4, Brushes.Gray, new double[] { 16, 10 });

        private static readonly LineStyle TimelineStyle = new LineStyle(0.5, Brushes.Black);
        private static readonly LineStyle ChannelStyle = new LineStyle(0.1, Brushes.Gray);

        private static readonly LineStyle SendData = new LineStyle(2, Brushes.Green);
        private static readonly LineStyle ReceiveData = new LineStyle(2, Brushes.Red);

        private const double ChannelHeight = 20;
        private const double JobHeight = 14;

        private const double TimeUnitWidth = 50;
        private const double Zero = 40;

        private double chartHeight;
        private Dictionary<Processor, double> timelineY = new Dictionary<Processor, double>();

        private void RecalculateTimelineY(IEnumerable<Processor> processors)
        {
            timelineY.Clear();
            double y = Zero + ChannelHeight * 2;

            foreach (var proc in processors)
            {
                timelineY[proc] = y;
                y += JobHeight + ChannelHeight * (options.PhysicalLinks[proc] + 1);
            }
            chartHeight = y - Zero;
        }

        private void PlaceLine(Line line, double left, double bottom, LineStyle style)
        {
            line.Stroke = style.Stroke;
            line.StrokeDashArray = style.Dashes;
            line.StrokeThickness = style.Thickness;

            pane.Children.Add(line);

            line.SetValue(Canvas.BottomProperty, bottom);
            line.SetValue(Canvas.LeftProperty, left);
        }

        private void PlaceLabel(LabelInfo label, double left, double bottom, AlignmentX x, AlignmentY y)
        {
            if (label == null)
                return;

            var textBlock = new TextBlock();

            textBlock.Text = label.Text;
            textBlock.Loaded += delegate
            {
                switch (x)
                {
                    case AlignmentX.Left: left -= (textBlock.ActualWidth + label.Padding); break;
                    case AlignmentX.Center: left -= textBlock.ActualWidth / 2; break;
                    case AlignmentX.Right: left += label.Padding; break;
                }

                switch (y)
                {
                    case AlignmentY.Bottom: bottom -= (textBlock.ActualHeight + label.Padding); break;
                    case AlignmentY.Center: bottom -= textBlock.ActualHeight / 2; break;
                    case AlignmentY.Top: bottom += label.Padding; break;
                }

                textBlock.SetValue(Canvas.BottomProperty, bottom);
                textBlock.SetValue(Canvas.LeftProperty, left);
            };
            pane.Children.Add(textBlock);
        }

        private void DrawVerticalLine(double height, double left, double bottom, LineStyle style, LabelInfo label = null)
        {
            var line = new Line
            {
                X1 = 0,
                X2 = 0,

                Y1 = 0,
                Y2 = height,
            };

            PlaceLine(line, left, bottom, style);
            PlaceLabel(label, left, bottom, AlignmentX.Center, AlignmentY.Bottom);
        }

        private void DrawHorizontalLine(double width, double left, double bottom, LineStyle style, LabelInfo label = null)
        {
            var line = new Line
            {
                X1 = 0,
                X2 = width,

                Y1 = 0,
                Y2 = 0
            };

            PlaceLine(line, left, bottom, style);
            PlaceLabel(label, left, bottom, AlignmentX.Left, AlignmentY.Center);
        }

        private void DrawAxes(double width)
        {
            DrawHorizontalLine(width, Zero, Zero, AxisStyle);
            DrawVerticalLine(chartHeight, Zero, Zero, AxisStyle);
        }

        private void DrawDataTransfer(DataTransfer transfer, Processor proc, bool flipped = false)
        {
            var outgoing = (transfer.FromProc == proc);
            var width = transfer.Duration * TimeUnitWidth;

            var label = new TextBlock
            {
                FontSize = 10,
                FontFamily = new FontFamily("Verdana"),

                Text = outgoing ?
                    string.Format("{0}-{1}({2})", transfer.FromOperation.ID, transfer.ToOperation.ID, transfer.ToProc.ID) :
                    string.Format("({0}){1}-{2}", transfer.FromProc.ID, transfer.FromOperation.ID, transfer.ToOperation.ID)
            };

            pane.Children.Add(label);

            var channel = outgoing ? transfer.FromProcChannel : transfer.ToProcChannel;

            var offsetY = timelineY[proc] + JobHeight + ChannelHeight * (channel + 1);
            var offsetX = Zero + transfer.StartTime * TimeUnitWidth;

            var serif = flipped ? 0 : -3;

            if (flipped)
            {
                offsetY -= 16;
            }

            label.SetValue(Canvas.BottomProperty, offsetY + 3);
            label.SetValue(Canvas.LeftProperty, offsetX);

            DrawHorizontalLine(width, offsetX, offsetY, outgoing ? SendData : ReceiveData);
            DrawVerticalLine(5, offsetX, offsetY + serif, outgoing ? SendData : ReceiveData);
            DrawVerticalLine(5, offsetX + transfer.Duration * TimeUnitWidth, offsetY + serif, outgoing ? SendData : ReceiveData);
        }

        private void DrawJobExecution(JobExecution jobExecution, Processor proc)
        {
            var width = jobExecution.Duration * TimeUnitWidth;

            var rect = new Rectangle
            {
                Height = JobHeight,
                Width = width,

                Stroke = Brushes.Black,
                StrokeThickness = 1.5
            };

            var label = new TextBlock
            {
                Height = rect.Height,
                Width = rect.Width,

                Text = jobExecution.Job.ID.ToString(),
                TextAlignment = TextAlignment.Center,
                FontSize = 11
            };

            pane.Children.Add(rect);
            pane.Children.Add(label);

            rect.SetValue(Canvas.BottomProperty, timelineY[proc]);
            rect.SetValue(Canvas.LeftProperty, Zero + jobExecution.StartTime * TimeUnitWidth);

            label.SetValue(Canvas.BottomProperty, timelineY[proc] + 1);
            label.SetValue(Canvas.LeftProperty, Zero + jobExecution.StartTime * TimeUnitWidth);
        }

        private void DrawTimeLine(Processor proc, double width)
        {
            DrawHorizontalLine(width, Zero, timelineY[proc], TimelineStyle, new LabelInfo(proc.ID.ToString(), 12, 16));

            foreach (var job in schedule[proc].OfType<JobExecution>())
            {
                DrawJobExecution(job, proc);
            }

            var transfers = schedule[proc].OfType<DataTransfer>().ToArray();
            var k = 0;

            while (k < transfers.Length - 1)
            {
                if (schedule.AreDuplex(transfers[k], transfers[k + 1]))
                {
                    DrawDataTransfer(transfers[k], proc);
                    DrawDataTransfer(transfers[k + 1], proc, true);
                    k += 2;
                }
                else
                {
                    DrawDataTransfer(transfers[k], proc);
                    k++;
                }
            }

            if (k == transfers.Length - 1)
                DrawDataTransfer(transfers[k], proc);
        }

        private void DrawInnerLines(Processor[] processors, double width)
        {
            var painted = new List<double>();

            foreach (var proc in processors)
            {
                /* Draw vertical lines through each timeline event finish moment */
                foreach (var period in schedule[proc])
                {
                    if (!painted.Contains(period.FinishTime))
                    {
                        DrawVerticalLine(chartHeight, Zero + period.FinishTime * TimeUnitWidth, Zero, InnerStyle,
                                         new LabelInfo(period.FinishTime.ToString(), 10, 13));
                        painted.Add(period.FinishTime);
                    }
                }

                /* Draw physical channels' timelines */
                for (int k = 0; k < options.PhysicalLinks[proc]; k++)
                {
                    var y = timelineY[proc] + JobHeight + ChannelHeight * (k + 1);
                    DrawHorizontalLine(width, Zero, y, ChannelStyle);
                    DrawHorizontalLine(4, Zero, y, new LineStyle(1, Brushes.Black));

                    PlaceLabel(new LabelInfo((k + 1).ToString(), 2, 11, new FontFamily("Verdana")), Zero + 5, y, AlignmentX.Right, AlignmentY.Center);
                }
            }
        }

        private void Paint(Schedule schedule, PcsOptions options, Canvas pane)
        {
            this.pane = pane;
            this.options = options;
            this.schedule = schedule;

            pane.Children.Clear();

            var processors = options.PhysicalLinks.Keys.ToArray();
            var finish = schedule.LastEvent<TimelinePeriodBase>().FinishTime;
            var width = TimeUnitWidth * (finish + 1);

            RecalculateTimelineY(processors);

            DrawAxes(width);
            DrawInnerLines(processors, width);
            DrawVerticalLine(chartHeight, Zero + finish * TimeUnitWidth, Zero, FinishStyle, new LabelInfo(finish.ToString(), 10, 13));

            foreach (var proc in processors.OrderBy(p => p.ID))
            {
                DrawTimeLine(proc, width);
            }
        }

        private Canvas pane;
        private Schedule schedule;
        private PcsOptions options;
    }
}
