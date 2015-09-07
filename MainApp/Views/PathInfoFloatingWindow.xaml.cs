using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using TaskPlanning.Queueing.Algorithms;
using TaskPlanning.Queueing.Statistics;

namespace MainApp.Views
{
    public partial class PathInfoFloatingWindow : Window
    {
        public PathInfoFloatingWindow()
        {
            InitializeComponent();

            var descriptor = DependencyPropertyDescriptor.FromProperty(Window.DataContextProperty, typeof(DataGrid));
            descriptor.AddValueChanged(this, DataContext_Changed);
        }

        void DataContext_Changed(object sender, EventArgs e)
        {
            Table.ItemsSource = (DataContext as TaskGraphStat).Vertices;
        }
    }
}
