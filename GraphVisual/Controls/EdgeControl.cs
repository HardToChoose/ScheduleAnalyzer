using GraphLogic.Entities;
using GraphVisual.InteractionEvents;
using Microsoft.Practices.Prism.PubSubEvents;

using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace GraphVisual.Controls
{
    public sealed class EdgeControl : Control
    {
        #region Dependency property shit

        static EdgeControl()
        {
            ShowArrowProperty = DependencyProperty.Register("ShowArrow", typeof(bool), typeof(EdgeControl), new PropertyMetadata(true));

            HasLabelPropertyKey = DependencyProperty.RegisterReadOnly("HasLabel", typeof(bool), typeof(EdgeControl), new PropertyMetadata(false));
            HasLabelProperty = HasLabelPropertyKey.DependencyProperty;

            StrokeThicknessProperty = DependencyProperty.Register("StrokeThickness", typeof(double), typeof(EdgeControl), new PropertyMetadata(2.0));
            StrokeBrushProperty = DependencyProperty.Register("StrokeBrush", typeof(Brush), typeof(EdgeControl), new PropertyMetadata(Brushes.Black));
            
            PathDataProperty = DependencyProperty.Register("PathData", typeof(PathGeometry), typeof(EdgeControl), new PropertyMetadata(null));
            ArrowPointsProperty = DependencyProperty.Register("ArrowPoints", typeof(PointCollection), typeof(EdgeControl), new PropertyMetadata(null));

            LabelProperty = DependencyProperty.Register("Label", typeof(EditableValueBase), typeof(EdgeControl), new PropertyMetadata(null, LabelPropertyChanged));
            LabelTemplateProperty = DependencyProperty.Register("LabelTemplate", typeof(DataTemplate), typeof(EdgeControl), new PropertyMetadata(null));

            LabelLeftProperty = DependencyProperty.Register("LabelLeft", typeof(double), typeof(EdgeControl), new PropertyMetadata(0.0));
            LabelTopProperty = DependencyProperty.Register("LabelTop", typeof(double), typeof(EdgeControl), new PropertyMetadata(0.0));

            DefaultStyleKeyProperty.OverrideMetadata(typeof(EdgeControl), new FrameworkPropertyMetadata(typeof(EdgeControl)));
        }

        public bool ShowArrow
        {
            get { return (bool)GetValue(ShowArrowProperty); }
            set { SetValue(ShowArrowProperty, value); }
        }

        public bool HasLabel
        {
            get { return (bool)GetValue(HasLabelProperty); }
        }

        public double StrokeThickness
        {
            get { return (double)GetValue(StrokeThicknessProperty); }
            set { SetValue(StrokeThicknessProperty, value); }
        }

        public Brush StrokeBrush
        {
            get { return (Brush)GetValue(StrokeBrushProperty); }
            set { SetValue(StrokeBrushProperty, value); }
        }

        public PathGeometry PathData
        {
            get { return (PathGeometry)GetValue(PathDataProperty); }
            set { SetValue(PathDataProperty, value); }
        }

        public PointCollection ArrowPoints
        {
            get { return (PointCollection)GetValue(ArrowPointsProperty); }
            set { SetValue(ArrowPointsProperty, value); }
        }

        public EditableValueBase Label
        {
            get { return (EditableValueBase)GetValue(LabelProperty); }
            set { SetValue(LabelProperty, value); }
        }

        public DataTemplate LabelTemplate
        {
            get { return (DataTemplate)GetValue(LabelTemplateProperty); }
            set { SetValue(LabelTemplateProperty, value); }
        }

        public double LabelLeft
        {
            get { return (double)GetValue(LabelLeftProperty); }
            private set { SetValue(LabelLeftProperty, value); }
        }

        public double LabelTop
        {
            get { return (double)GetValue(LabelTopProperty); }
            private set { SetValue(LabelTopProperty, value); }
        }

        public static readonly DependencyProperty ShowArrowProperty;
        private static readonly DependencyPropertyKey HasLabelPropertyKey;
        public static readonly DependencyProperty HasLabelProperty;
        public static readonly DependencyProperty StrokeThicknessProperty;
        public static readonly DependencyProperty StrokeBrushProperty;
        public static readonly DependencyProperty PathDataProperty;
        public static readonly DependencyProperty ArrowPointsProperty;
        public static readonly DependencyProperty LabelProperty;
        public static readonly DependencyProperty LabelTemplateProperty;
        public static readonly DependencyProperty LabelLeftProperty;
        public static readonly DependencyProperty LabelTopProperty;

        private static void LabelPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var edge = d as EdgeControl;
            var vector = (edge.End != null && edge.Start != null) ?
                new Vector(edge.End.CenterPosition.X - edge.Start.CenterPosition.X,
                           edge.End.CenterPosition.Y - edge.Start.CenterPosition.Y) : new Vector();

            edge.PlaceLabel(vector);
            edge.SetValue(HasLabelPropertyKey, e.NewValue != null);
        }

        #endregion

        public IEventAggregator Aggregator { get; set; }

        public VertexControl Start { get; private set; }
        public VertexControl End { get; private set; }

        public EdgeControl()
        {
            Aggregator = new EventAggregator();

            LayoutUpdated += delegate
            {
                Repaint();
            };

            MouseUp += (s, e) =>
            {
                e.Handled = true;

                if (e.ChangedButton == MouseButton.Middle)
                    Aggregator.GetEvent<EdgeEditBegan>().Publish(this);
            };
        }

        public EdgeControl(VertexControl start, VertexControl end, EditableValueBase label = null) : this()
        {
            PathData = new PathGeometry();
            ArrowPoints = new PointCollection();

            Label = label;
            Start = start;
            End = end;
        }

        public bool HasVertex(VertexControl vertex)
        {
            return (Start == vertex) || (End == vertex);
        }

        private void GetArrowSides(Vector height, double baseWidth, out Vector side_1, out Vector side_2)
        {
            var y = Math.Sqrt(Math.Pow(baseWidth * height.X / 2, 2) / height.LengthSquared);

            side_1 = height + new Vector(-y * height.Y / height.X, y);
            side_2 = height + new Vector(y * height.Y / height.X, -y);

            if (double.IsNaN(side_1.X))     side_1.X = -baseWidth / 2;
            if (double.IsNaN(side_2.X))     side_2.X =  baseWidth / 2;           
        }

        private Point GetContactPoint(Point source, Point target, double boxHalfWidth)
        {
            double DX = target.X - source.X;
            double DY = target.Y - source.Y;

            double dx, dy;

            if (Math.Abs(DY) > Math.Abs(DX))
            {
                dy = (DY < 0) ? boxHalfWidth : -boxHalfWidth;
                dx = (DX == 0) ? 0 : dy * DX / DY;
            }
            else
            {
                dx = (DX < 0) ? boxHalfWidth : -boxHalfWidth;
                dy = (DY == 0) ? 0 : dx * DY / DX;
            }
            return new Point(target.X + dx, target.Y + dy);
        }

        public void Repaint()
        {
            var startPoint = Start.CenterPosition;
            var endPoint = End.CenterPosition;

            var edgeVector = new Vector(endPoint.X - startPoint.X, endPoint.Y - startPoint.Y);

            if (ShowArrow)
            {
                var arrowHeight = StrokeThickness * 10;
                var angle = 40 * Math.PI / 180;

                var heightVector = edgeVector;
                    heightVector.Normalize();
                    heightVector *= arrowHeight;

                Vector side_1, side_2;
                GetArrowSides(heightVector, heightVector.Length * Math.Sin(angle), out side_1, out side_2);

                var contact = GetContactPoint(startPoint, endPoint, End.CanvasBoundingBox.Width / 2);
                ArrowPoints = new PointCollection();

                ArrowPoints.Add(contact);
                ArrowPoints.Add(Point.Subtract(contact, side_1));
                ArrowPoints.Add(Point.Subtract(contact, side_2));

                endPoint = contact;
            }

            PathData.Figures.Clear();
            PathData.Figures.Add(new PathFigure(startPoint, new[] { new LineSegment(endPoint, true) }, false));

            PlaceLabel(edgeVector);
        }

        private void PlaceLabel(Vector edgeVector)
        {
            if (Template != null)
            {
                var label = Template.FindName("EdgeLabel", this) as FrameworkElement;

                if (label != null)
                {
                    var bounds = PathData.Bounds;

                    LabelLeft = bounds.Left + (bounds.Width - label.ActualWidth) / 2;
                    LabelTop = bounds.Top + (bounds.Height - label.ActualHeight) / 2;
                }
            }
        }
    }
}
