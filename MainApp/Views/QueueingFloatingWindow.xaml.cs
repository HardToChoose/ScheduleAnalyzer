using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

using TaskPlanning.Queueing;
using UI.Converters;

namespace MainApp.Views
{
    public partial class QueueingFloatingWindow : Window
    {
        public QueueingFloatingWindow()
        {
            InitializeComponent();

            var descriptor = DependencyPropertyDescriptor.FromProperty(Window.DataContextProperty, typeof(DataGrid));
            descriptor.AddValueChanged(this, DataContext_Changed);
        }

        void DataContext_Changed(object sender, EventArgs e)
        {
            Table.ItemsSource = (DataContext as OperationQueue).Items;

            ClearColumns();
            AddTaskQueueColumns();
        }

        void QueueingFloatingWindow_Activated(object sender, EventArgs e)
        {
            if (IsActive)
            {
                AddTaskQueueColumns();
            }
            else
            {
                ClearColumns();
            }
        }

        private void ClearColumns()
        {
            Table.Columns.Clear();
        }

        private void AddTaskQueueColumns()
        {
            var queue = DataContext as OperationQueue;

            Table.Columns.Add(new DataGridTextColumn
            {
                Header = "№",
                Binding = new Binding
                {
                    Path = new PropertyPath("DataContext"),
                    RelativeSource = new RelativeSource(RelativeSourceMode.Self),
                    Converter = new QueueItemToIndex(),
                    ConverterParameter = queue
                }
            });

            Table.Columns.Add(new DataGridTextColumn
            {
                Header = "Вершина",
                Binding = new Binding("Value.ID")
            });


            for (int k = 0; k < queue.ParameterNames.Length; k++)
            {
                Table.Columns.Add(new DataGridTextColumn
                {
                    Header = queue.ParameterNames[k],
                    Binding = new Binding(string.Format("Parameters[{0}]", k))
                });
            }

            foreach (var column in Table.Columns)
            {
                column.IsReadOnly = true;

                column.CanUserReorder = false;
                column.CanUserResize = false;
                column.CanUserSort = false;
            }
        }
    }
}
