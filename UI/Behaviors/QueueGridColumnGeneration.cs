using System.Linq;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Interactivity;

using TaskPlanning.Queueing;

namespace UI.Behaviors
{
    public class QueueGridColumnGeneration : Behavior<DataGrid>
    {
        private bool autoGenerateColumnsOriginal;
        private DataGridColumn[] oldColumns;

        protected override void OnAttached()
        {
            AssociatedObject.IsVisibleChanged += (s, e) =>
            {
                var visible = (bool)e.NewValue;
                if (visible == true)
                {
                    autoGenerateColumnsOriginal = AssociatedObject.AutoGenerateColumns;
                    oldColumns = AssociatedObject.Columns.ToArray();

                    AssociatedObject.Columns.Clear();
                    AddTaskQueueColumns();
                }
                else
                {
                    Clear();
                }
            };
        }

        private void Clear()
        {
            AssociatedObject.AutoGenerateColumns = autoGenerateColumnsOriginal;
            AssociatedObject.Columns.Clear();

            foreach (var column in oldColumns)
                AssociatedObject.Columns.Add(column);
        }

        protected override void OnDetaching() { }

        private void AddTaskQueueColumns()
        {
            var queue = AssociatedObject.DataContext as OperationQueue;
            if (queue == null)
                return;

            AssociatedObject.ItemsSource = queue.Items;
            AssociatedObject.Columns.Add(new DataGridTextColumn
            {
                Header = "№",
                Binding = new Binding("Value.ID"),

                IsReadOnly = true,

                CanUserReorder = false,
                CanUserResize = false,
                CanUserSort = false
            });

            
            for (int k = 0; k < queue.ParameterNames.Length; k++)
            {
                AssociatedObject.Columns.Add(new DataGridTextColumn
                {
                    Header = queue.ParameterNames[k],
                    Binding = new Binding(string.Format("Parameters[{0}]", k)),

                    IsReadOnly = true,

                    CanUserReorder = false,
                    CanUserResize = false,
                    CanUserSort = false
                });
            }
        }
    }
}
