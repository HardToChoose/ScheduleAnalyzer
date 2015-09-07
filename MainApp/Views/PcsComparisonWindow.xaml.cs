using MainApp.ViewModels;
using MainApp.ViewModels.AlgorithmComparison;

using OxyPlot;
using OxyPlot.Wpf;

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace MainApp.Views
{
    public partial class PcsComparisonWindow : Window
    {
        public PcsComparisonWindow()
        {
            InitializeComponent();
        }

        private void DataGrid_Initialized(object sender, EventArgs e)
        {
            var dataGrid = sender as DataGrid;
            if (dataGrid == null)
                return;

            UpdateData(dataGrid);
        }

        private void UpdateData(DataGrid dataGrid)
        {
            var table = dataGrid.DataContext as Table;
            if (table == null)
                return;

            dataGrid.Columns.Clear();

            for (int j = 0; j < table.JobCounts.Count; j++)
            {
                dataGrid.Columns.Add(new DataGridTextColumn
                {
                    Header = table.JobCounts[j].ToString(),
                    Binding = new Binding(string.Format("Items[{0}]", j))
                    {
                        StringFormat = "{0:0.###}"
                    }
                });
            }
        }

        private void PlotView_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            var model = PlotCtrl.Model;
            switch (e.Key)
            {
                case Key.Q: model.LegendPosition = LegendPosition.TopLeft; break;
                case Key.W: model.LegendPosition = LegendPosition.TopCenter; break;
                case Key.E: model.LegendPosition = LegendPosition.TopRight; break;

                case Key.A: model.LegendPosition = LegendPosition.LeftMiddle; break;
                case Key.D: model.LegendPosition = LegendPosition.RightMiddle; break;

                case Key.Z: model.LegendPosition = LegendPosition.BottomLeft; break;
                case Key.X: model.LegendPosition = LegendPosition.BottomCenter; break;
                case Key.C: model.LegendPosition = LegendPosition.BottomRight; break;
            }
            model.InvalidatePlot(false);
        }
    }
}
