using System;
using System.Windows;
using System.Windows.Controls;

namespace UI.Controls
{
    public class ScrollableCanvasControl : Canvas
    {
        static ScrollableCanvasControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ScrollableCanvasControl), new FrameworkPropertyMetadata(typeof(ScrollableCanvasControl)));
        }

        protected override Size MeasureOverride(Size constraint)
        {
            double bottom = 0.0;
            double right = 0.0;

            foreach (object obj in Children)
            {
                var child = obj as FrameworkElement;

                if (child != null)
                {
                    child.Measure(constraint);
                    
                    var childBottom = GetTop(child) + child.DesiredSize.Height;
                    var childRight = GetLeft(child) + child.DesiredSize.Width;
                    
                    if (double.IsNaN(childBottom))
                        childBottom = GetBottom(child);

                    if (double.IsNaN(childRight))
                        childRight = GetRight(child);

                    if (double.IsNaN(childRight) || double.IsNaN(childBottom))
                        continue;

                    bottom = Math.Max(bottom, childBottom);
                    right = Math.Max(right, childRight);
                }
            }
            return new Size(right, bottom);
        }
    }
}
