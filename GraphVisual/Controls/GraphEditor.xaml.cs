using GraphLogic.Entities;
using GraphLogic.Graphs;
using GraphLogic.Layouts;

using GraphVisual.Converters;
using GraphVisual.InteractionEvents;

using Microsoft.Practices.Prism.PubSubEvents;
using Microsoft.Win32;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace GraphVisual.Controls
{
    public partial class GraphEditor : UserControl
    {
        #region Dependency properties

        public double Scale
        {
            get { return (double)GetValue(ScaleProperty); }
            set { SetValue(ScaleProperty, value); }
        }

        public ValueEditorBase VertexDataEditor
        {
            get { return (ValueEditorBase)GetValue(VertexDataEditorProperty); }
            set { SetValue(VertexDataEditorProperty, value); }
        }

        public ValueEditorBase EdgeLabelEditor
        {
            get { return (ValueEditorBase)GetValue(EdgeLabelEditorProperty); }
            set { SetValue(EdgeLabelEditorProperty, value); }
        }

        public IWeightedGraph Graph
        {
            get { return (IWeightedGraph)GetValue(GraphProperty); }
            set { SetValue(GraphProperty, value); }
        }

        public GraphInfo GraphInfo
        {
            get { return (GraphInfo)GetValue(GraphInfoProperty); }
            set { SetValue(GraphInfoProperty, value); }
        }

        private static readonly DependencyProperty ScaleProperty =
            DependencyProperty.Register("Scale", typeof(double), typeof(GraphEditor), new PropertyMetadata(1.0));

        public static readonly DependencyProperty VertexDataEditorProperty =
            DependencyProperty.Register("VertexDataEditor", typeof(ValueEditorBase), typeof(ValueEditorDialog), new PropertyMetadata(null));

        public static readonly DependencyProperty EdgeLabelEditorProperty =
            DependencyProperty.Register("EdgeLabelEditor", typeof(ValueEditorBase), typeof(ValueEditorDialog), new PropertyMetadata(null));

        public static readonly DependencyProperty GraphProperty =
            DependencyProperty.Register("Graph", typeof(IWeightedGraph), typeof(GraphEditor), new PropertyMetadata(null, GraphChanged));

        public static readonly DependencyProperty GraphInfoProperty =
            DependencyProperty.Register("GraphInfo", typeof(GraphInfo), typeof(GraphEditor), new PropertyMetadata(null));

        private static void GraphChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as GraphEditor).GraphChanged();
        }

        #endregion

        #region Private fields

        private LinearToLogConverter conv = new LinearToLogConverter();

        private List<VertexControl> vertexControls = new List<VertexControl>();
        private List<EdgeControl> edgeControls = new List<EdgeControl>();

        private int currentZIndex = 1;

        private ScaleTransform scale = new ScaleTransform();
        private TranslateTransform translate = new TranslateTransform();

        private VertexControl selectedVertex;
        private Window parentWindow;

        private IEventAggregator aggregator = new EventAggregator();

        #endregion

        public GraphEditor()
        {
            InitializeComponent();
            SubscribeToEvents();

            GraphInfo = new GraphInfo
            {
                Layout = new CircleLayout(),
                LayoutIcon = Resources["CircleIcon"] as ImageSource
            };
        }

        private void SubscribeToEvents()
        {
            ScaleSlider.ValueChanged += (s, e) => Slider_ValueChanged(e.NewValue);
            GraphCanvas.SizeChanged += (s, e) => FitToContent();

            Loaded += GraphEditor_Loaded;

            aggregator.GetEvent<VertexDragBegan>().Subscribe(BringToFront, ThreadOption.UIThread);
            aggregator.GetEvent<VertexMovedEvent>().Subscribe(UpdateEdgesWithVertex, ThreadOption.UIThread);

            aggregator.GetEvent<VertexEditBegan>().Subscribe(EditVertex, ThreadOption.UIThread);
            aggregator.GetEvent<VertexRemovedEvent>().Subscribe(RemoveVertex, ThreadOption.UIThread);
            aggregator.GetEvent<VertexSelectedEvent>().Subscribe(DeselectVertex, ThreadOption.UIThread);
            aggregator.GetEvent<VertexDeselectedEvent>().Subscribe(SelectVertex, ThreadOption.UIThread);

            aggregator.GetEvent<EdgeEditBegan>().Subscribe(EditEdgeLabel, ThreadOption.UIThread);
        }

        private void ShowGraph(bool show)
        {
            GraphCanvas.Visibility = show ? Visibility.Visible : Visibility.Hidden;
        }

        private void ClearControls()
        {
            GraphCanvas.Children.Clear();

            vertexControls.Clear();
            edgeControls.Clear();

            currentZIndex = 1;

            Scale = 1.0;
            Translation.X = 0;
            Translation.Y = 0;
            
        }

        #region Layouting

        private void ApplyLayout()
        {
            if (GraphInfo.Layout != null && Graph.VertexCount != 0)
            {
                var vertexSizes = new Dictionary<EditableValueBase, Size>();
                foreach (var vertex in vertexControls)
                {
                    vertexSizes[vertex.Data] = vertex.RenderSize;
                }
                var positions = GraphInfo.Layout.Compute(Graph, vertexSizes);

                if (positions != null)
                {
                    var minX = positions.Values.Min(pos => pos.X);
                    var minY = positions.Values.Min(pos => pos.Y);

                    foreach (var vertex in vertexControls)
                    {
                        Canvas.SetLeft(vertex, positions[vertex.Data].X - minX);
                        Canvas.SetTop(vertex, positions[vertex.Data].Y - minY);
                    }                    
                }
            }
        }

        private void FitToContent()
        {
            if (vertexControls.Count == 0)
                return;

            var paneHeight = vertexControls.Max(v => Canvas.GetTop(v) + v.ActualHeight) - vertexControls.Min(v => Canvas.GetTop(v));
            var paneWidth = vertexControls.Max(v => Canvas.GetLeft(v) + v.ActualWidth) - vertexControls.Min(v => Canvas.GetLeft(v));

            var scaleX = GraphCanvas.ActualWidth / paneWidth;
            var scaleY = GraphCanvas.ActualHeight / paneHeight;
            var scale = (scaleX < scaleY) ? scaleX : scaleY;

            Scale = scale * 0.92;
            var middle = (ScaleSlider.Maximum - ScaleSlider.Minimum) / 2;

            if (Scale >= 1)
            {
                ScaleSlider.Value = (Scale - 1) * (ScaleSlider.Minimum - middle) + middle;
            }
            else
            {
                ScaleSlider.Value = (Scale - ((Scale > 0.1) ? 0.1 : 0)) * middle * 0.9;
            }

            Translation.X = (GraphCanvas.ActualWidth - paneWidth) / 2;
            Translation.Y = (GraphCanvas.ActualHeight - paneHeight) / 2;
        }

        #endregion

        #region Load & Save stuff

        private void SaveGraph(string fileName)
        {
            var vertices = new XElement("vertices");
            var layout = new XElement("layout");
            var edges = new XElement("edges");
            
            var root = new XElement("graph", vertices, edges, layout);

            var vertexWriter = vertices.CreateWriter();
            var edgeWriter = edges.CreateWriter();

            foreach (var vertex in vertexControls)
            {
                WriteVertex(vertex.Data, vertexWriter);
                layout.Add(XmlVertexPosition(vertex));
            }

            foreach (var edge in edgeControls)
                WriteEdge(edge, edgeWriter);

            edgeWriter.Close();
            vertexWriter.Close();

            new XDocument(root).Save(fileName);
        }

        /// <summary>
        /// !! Able to throw a lot of exceptions
        /// </summary>
        /// <param name="fileName"></param>
        public void LoadGraph(string fileName)
        {
            var root = XDocument.Load(fileName, LoadOptions.None).Root;

            foreach (var vertex in root.Element("vertices").Elements())
            {
                var data = Activator.CreateInstance(GraphInfo.VertexDataType) as EditableValueBase;
                data.ReadXml(vertex.CreateReader());

                AddVertex(data);
            }

            int k = 0;
            foreach (var position in root.Element("layout").Elements())
            {
                var vertex = vertexControls[k++];
                vertex.Loaded += delegate
                {
                    Canvas.SetLeft(vertex, double.Parse(position.Attribute("X").Value, CultureInfo.InvariantCulture));
                    Canvas.SetTop(vertex, double.Parse(position.Attribute("Y").Value, CultureInfo.InvariantCulture));
                };
            }

            foreach (var edge in root.Element("edges").Elements())
            {
                var label = new WeightedEdge(null, null);
                label.ReadXml(edge.Element("label").CreateReader());

                AddEdge(vertexControls[int.Parse(edge.Attribute("Source").Value)],
                        vertexControls[int.Parse(edge.Attribute("Target").Value)],
                        label.Weight);
            }
        }

        private void WriteVertex(IXmlSerializable vertex, XmlWriter writer)
        {
            writer.WriteStartElement("vertex");
            vertex.WriteXml(writer);
            writer.WriteEndElement();
        }

        private XElement XmlVertexPosition(VertexControl vertex)
        {
            return new XElement("position",
                new XAttribute("X", Canvas.GetLeft(vertex)),
                new XAttribute("Y", Canvas.GetTop(vertex)));
        }

        private void WriteEdge(EdgeControl edge, XmlWriter writer)
        {
            writer.WriteStartElement("edge");
            {
                writer.WriteAttributeString("Source", vertexControls.IndexOf(edge.Start).ToString());
                writer.WriteAttributeString("Target", vertexControls.IndexOf(edge.End).ToString());

                writer.WriteStartElement("label");
                {
                    edge.Label.WriteXml(writer);
                }
                writer.WriteEndElement();
            }
            writer.WriteEndElement();
        }

        #endregion

        #region Vertex operations

        private int GetNextProcessorID()
        {
            if (Graph.VertexCount == 0)
                return 1;

            var sequence = Enumerable.Range(1, Graph.Vertices.Max(vertex => vertex.ID));
            var ids = Graph.Vertices.Select(vertex => vertex.ID);

            var result = sequence.Except(ids).FirstOrDefault();
            return (result == 0) ? Graph.VertexCount + 1: result;
        }

        private void CreateVertex(Point canvasPosition, Point windowPosition)
        {
            if (VertexDataEditor == null)
                return;

            var newOperation = VertexDataEditor.GetDefaultValue(GetNextProcessorID());
            var vertex = AddVertex(newOperation, canvasPosition);

            var editor = new ValueEditorDialog(VertexDataEditor, newOperation)
            {
                Owner = parentWindow,
                Title = "Нова вершина"
            };

            editor.Loaded += delegate
            {
                var position = vertex.TranslatePoint(new Point(vertex.ActualWidth + 20, -editor.ActualHeight / 2), parentWindow);

                editor.Left = position.X + parentWindow.Left;
                editor.Top = position.Y + parentWindow.Top;
            };

            if (editor.ShowDialog() != true)
            {
                RemoveVertex(vertex);
            }
        }

        private VertexControl AddVertex(EditableValueBase data, Point centerCoords = default(Point))
        {
            var vertex = VertexControl.Create(aggregator, data);
            vertex.Visibility = Visibility.Hidden;

            vertex.Loaded += delegate
            {
                Canvas.SetTop(vertex, centerCoords.Y - vertex.ActualHeight / 2);
                Canvas.SetLeft(vertex, centerCoords.X - vertex.ActualWidth / 2);
                Canvas.SetZIndex(vertex, currentZIndex);

                vertex.Visibility = Visibility.Visible;
            };

            GraphCanvas.Children.Add(vertex);
            vertexControls.Add(vertex);

            if (Graph != null)
            {
                Graph.AddVertex(data);
            }
            return vertex;
        }

        private void BringToFront(VertexControl vertex)
        {
            Canvas.SetZIndex(vertex, ++currentZIndex);
        }

        private void DeselectVertex(VertexControl vertex)
        {
            if (selectedVertex == vertex)
                selectedVertex = null;
        }

        private void SelectVertex(VertexControl vertex)
        {
            if (selectedVertex == null)
            {
                selectedVertex = vertex;
            }
            else
            {
                if (!TryRemoveEdge(selectedVertex, vertex))
                {
                    AddEdge(selectedVertex, vertex);
                }

                selectedVertex.Deselect();
                vertex.Deselect();
                
                selectedVertex = null;
            }
        }

        private void EditVertex(VertexControl vertex)
        {
            if (VertexDataEditor == null)
                return;

            var operationCopy = vertex.Data.DeepCopy();
            var editor = new ValueEditorDialog(VertexDataEditor, vertex.Data)
            {
                Owner = parentWindow,
                Title = "Змінити вершину"
            };

            if (editor.ShowDialog() != true)
            {
                vertex.Data = operationCopy;
            }
        }

        private void RemoveVertex(VertexControl vertex)
        {
            if (vertex == null)
                return;
            RemoveEdgesWithVertex(vertex);

            vertexControls.Remove(vertex);
            GraphCanvas.Children.Remove(vertex);

            if (Graph != null)
            {
                Graph.RemoveVertex(vertex.Data);
            }

            if (selectedVertex == vertex)
                selectedVertex = null;
        }

        #endregion

        #region Edge operations

        private bool EdgeExists(VertexControl start, VertexControl end)
        {
            return edgeControls.Exists(edge => edge.Start == start && edge.End == end) ||
                   !Graph.IsDirected &&
                   edgeControls.Exists(edge => edge.Start == end && edge.End == start);
        }

        private void CreateEdgeControl(WeightedEdge graphEdge)
        {
            var edgeControl = new EdgeControl(vertexControls.FirstOrDefault(v => v.Data == graphEdge.Source),
                                              vertexControls.FirstOrDefault(v => v.Data == graphEdge.Target), graphEdge)
            {
                Aggregator = aggregator,
                ShowArrow = (Graph != null) && Graph.IsDirected
            };
            Canvas.SetZIndex(edgeControl, 0);

            edgeControls.Add(edgeControl);
            GraphCanvas.Children.Add(edgeControl);
        }

        private void AddEdge(VertexControl start, VertexControl end, double weight = 1)
        {
            var graphEdge = new WeightedEdge(start.Data, end.Data, weight);
            var edge = new EdgeControl(start, end, graphEdge)
            {
                Aggregator = aggregator,
                ShowArrow = (Graph != null) && Graph.IsDirected
            };

            edgeControls.Add(edge);
            GraphCanvas.Children.Add(edge);

            if (Graph != null)
            {
                Graph.AddEdge(graphEdge);
            }
        }

        private void EditEdgeLabel(EdgeControl edge)
        {
            if (EdgeLabelEditor == null)
                return;

            var labelCopy = edge.Label.DeepCopy();
            var editor = new ValueEditorDialog(EdgeLabelEditor, edge.Label)
            {
                Owner = parentWindow,
                Title = "Змінити ребро"
            };

            if (editor.ShowDialog() != true)
            {
                edge.Label = labelCopy;
            }
        }

        private bool RemoveEdge(WeightedEdge edge)
        {
            return RemoveEdge(edgeControls.FirstOrDefault(e => e.Label == edge));
        }

        private bool RemoveEdge(EdgeControl edgeControl)
        {
            if (edgeControl == null)
                return false;

            GraphCanvas.Children.Remove(edgeControl);
            edgeControls.Remove(edgeControl);

            return (Graph != null) && Graph.RemoveEdge(edgeControl.Label as WeightedEdge);
        }

        private bool TryRemoveEdge(VertexControl start, VertexControl end)
        {
            if (RemoveEdge(edgeControls.FirstOrDefault(e => e.Start == start && e.End == end)))
            {
                return true;
            }
            return !Graph.IsDirected && RemoveEdge(edgeControls.FirstOrDefault(e => e.Start == end && e.End == start));
        }

        private void RemoveEdgesWithVertex(VertexControl vertex)
        {
            for (int k = 0; k < edgeControls.Count; )
            {
                if (edgeControls[k].HasVertex(vertex))
                    RemoveEdge(edgeControls[k]);
                else
                    k++;
            }
        }

        private void UpdateEdgesWithVertex(VertexControl vertex)
        {
            foreach (var edge in edgeControls.Where(e => e.Start == vertex || e.End == vertex))
            {
                edge.Repaint();
            }
        }

        #endregion

        #region Button click handlers

        private OpenFileDialog openFileDialog = new OpenFileDialog();
        private SaveFileDialog saveFileDialog = new SaveFileDialog();

        private void LoadButton_Click(object sender, RoutedEventArgs e)
        {
            openFileDialog.Filter = GraphInfo.FileExtensions;

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    ClearButton_Click(null, null);
                    LoadGraph(openFileDialog.FileName);

                    Dispatcher.BeginInvoke(DispatcherPriority.Render, new Action(FitToContent));
                }
                catch { }
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            saveFileDialog.Filter = GraphInfo.FileExtensions;

            if (saveFileDialog.ShowDialog() == true)
            {
                SaveGraph(saveFileDialog.FileName);
            }
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            ClearControls();

            if (Graph != null)
            {
                Graph.Clear();
            }
        }

        private void LayoutButton_Click(object sender, RoutedEventArgs e)
        {
            ApplyLayout();
        }

        private void FillButton_Click(object sender, RoutedEventArgs e)
        {
            FitToContent();
        }

        #endregion

        #region Other event handlers

        private void GraphEditor_Loaded(object sender, RoutedEventArgs e)
        {
            parentWindow = Window.GetWindow(this);
        }

        private void Slider_ValueChanged(double value)
        {
            var middle = (ScaleSlider.Maximum - ScaleSlider.Minimum) / 2;

            if (value >= middle)
            {
                Scale = 1 + (value - middle) / (ScaleSlider.Maximum - middle);
            }
            else
            {
                Scale = value / middle * 0.9 + 0.1;
            }
        }

        private void Canvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                var canvasPosition = e.GetPosition(GraphCanvas);
                var windowPosition = e.GetPosition(parentWindow);

                CreateVertex(canvasPosition, windowPosition);
            }
        }

        private void GraphChanged()
        {
            GraphCanvas.Visibility = Visibility.Hidden;
            ClearControls();

            if (Graph != null)
            {
                foreach (var vertex in Graph.Vertices)
                {
                    var vertexControl = VertexControl.Create(aggregator, vertex);
                    Canvas.SetZIndex(vertexControl, currentZIndex);

                    GraphCanvas.Children.Add(vertexControl);
                    vertexControls.Add(vertexControl);
                }

                foreach (var edge in Graph.Edges)
                {
                    CreateEdgeControl(edge);
                }                

                GraphCanvas.UpdateLayout();
                ApplyLayout();
            }

            GraphCanvas.Visibility = Visibility.Visible;
            FitToContent();
        }

        #endregion     
    }
}
