using GraphLogic.Entities;
using GraphVisual.InteractionEvents;
using Microsoft.Practices.Prism.PubSubEvents;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace GraphVisual.Controls
{
    public sealed class VertexControl : Control
    {
        #region Dependency property shit

        static VertexControl()
        {
            DataProperty = DependencyProperty.Register("Data", typeof(EditableValueBase), typeof(VertexControl), new PropertyMetadata(null));
            DataTemplateProperty = DependencyProperty.Register("DataTemplate", typeof(DataTemplate), typeof(VertexControl), new PropertyMetadata(null));

            IsSelectedPropertyKey = DependencyProperty.RegisterReadOnly("IsSelected", typeof(bool), typeof(VertexControl), new PropertyMetadata(false));
            IsSelectedProperty = IsSelectedPropertyKey.DependencyProperty;

            HighlightBorderBrushProperty = DependencyProperty.Register("HighlightBorderBrush", typeof(Brush), typeof(VertexControl), new PropertyMetadata(Brushes.Blue));
            HighlightBorderThicknessProperty = DependencyProperty.Register("HighlightBorderThickness", typeof(Thickness), typeof(VertexControl), new PropertyMetadata(new Thickness(1)));

            DefaultStyleKeyProperty.OverrideMetadata(typeof(VertexControl), new FrameworkPropertyMetadata(typeof(VertexControl)));

            Canvas.TopProperty.OverrideMetadata(typeof(VertexControl), new FrameworkPropertyMetadata((d, e) => (d as VertexControl).PositionChanged()));
            Canvas.LeftProperty.OverrideMetadata(typeof(VertexControl), new FrameworkPropertyMetadata((d, e) => (d as VertexControl).PositionChanged()));
        }

        public EditableValueBase Data
        {
            get { return (EditableValueBase)GetValue(DataProperty); }
            set { SetValue(DataProperty, value); }
        }
        
        public DataTemplate DataTemplate
        {
            get { return (DataTemplate)GetValue(DataTemplateProperty); }
            set { SetValue(DataTemplateProperty, value); }
        }
        
        public bool IsSelected
        {
            get { return (bool)GetValue(IsSelectedProperty); }
        }

        public Brush HighlightBorderBrush
        {
            get { return (Brush)GetValue(HighlightBorderBrushProperty); }
            set { SetValue(HighlightBorderBrushProperty, value); }
        }

        public Thickness HighlightBorderThickness
        {
            get { return (Thickness)GetValue(HighlightBorderThicknessProperty); }
            set { SetValue(HighlightBorderThicknessProperty, value); }
        }

        public static readonly DependencyProperty DataProperty;
        public static readonly DependencyProperty DataTemplateProperty;

        public static readonly DependencyProperty IsSelectedProperty;
        private static readonly DependencyPropertyKey IsSelectedPropertyKey;

        public static readonly DependencyProperty HighlightBorderBrushProperty;
        public static readonly DependencyProperty HighlightBorderThicknessProperty;

        #endregion

        public static VertexControl Create(IEventAggregator aggregator, EditableValueBase data)
        {
            return new VertexControl
            {
                Aggregator = aggregator,
                Data = data
            };
        }

        private Point? dragDiff;

        public IEventAggregator Aggregator { get; set; }

        public Rect CanvasBoundingBox
        {
            get
            {
                return new Rect(Canvas.GetLeft(this), Canvas.GetTop(this), ActualWidth, ActualHeight);
            }
        }

        public Point CenterPosition
        {
            get
            {
                return new Point(Canvas.GetLeft(this) + ActualWidth / 2,
                                 Canvas.GetTop(this) + ActualHeight / 2);
            }
        }

        public VertexControl()
        {
            Aggregator = new EventAggregator();

            MouseUp += MiddleButtonUp;

            MouseDoubleClick += DoubleClick;
            MouseRightButtonUp += RightButtonUp;

            MouseLeftButtonDown += LeftButtonDown;
            MouseMove += MouseMoveHandler;
            MouseLeftButtonUp += LeftButtonUp;
        }

        public void Deselect()
        {
            SetValue(IsSelectedPropertyKey, false);
        }

        #region Event handlers

        private void MiddleButtonUp(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;

            if (e.ChangedButton == MouseButton.Middle)
            {
                Aggregator.GetEvent<VertexEditBegan>().Publish(this);
            }
        }

        private void DoubleClick(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;

            if (e.ChangedButton == MouseButton.Left)
            {
                Aggregator.GetEvent<VertexRemovedEvent>().Publish(this);
            }
        }

        private void RightButtonUp(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;

            if (IsSelected)
            {
                SetValue(IsSelectedPropertyKey, false);
                Aggregator.GetEvent<VertexSelectedEvent>().Publish(this);
            }
            else
            {
                SetValue(IsSelectedPropertyKey, true);
                Aggregator.GetEvent<VertexDeselectedEvent>().Publish(this);
            }
        }

        #region Drag behavior

        private void LeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var mousePosition = e.GetPosition(Parent as Canvas);
            var vertexPosition = new Point(Canvas.GetLeft(this), Canvas.GetTop(this));

            dragDiff = new Point(vertexPosition.X - mousePosition.X, vertexPosition.Y - mousePosition.Y);
            CaptureMouse();

            Aggregator.GetEvent<VertexDragBegan>().Publish(this);
        }

        private void MouseMoveHandler(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && dragDiff.HasValue)
            {
                var mousePosition = e.GetPosition(Parent as Canvas);

                Canvas.SetLeft(this, mousePosition.X + dragDiff.Value.X);
                Canvas.SetTop(this, mousePosition.Y + dragDiff.Value.Y);
            }
        }

        private void LeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            ReleaseMouseCapture();
            dragDiff = null;
        }

        #endregion

        private void PositionChanged()
        {
            Aggregator.GetEvent<VertexMovedEvent>().Publish(this);
        }

        #endregion
    }
}
