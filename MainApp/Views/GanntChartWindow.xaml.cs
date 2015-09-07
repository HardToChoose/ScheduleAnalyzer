using System.Windows;

using TaskPlanning.JobAssignment;

namespace MainApp.Views
{
    public partial class GanntChartWindow : Window
    {
        public GanntChartWindow(PcsOptions pcsOptions, Schedule schedule)
        {
            InitializeComponent();

            Chart.Options = pcsOptions;
            Chart.Schedule = schedule;
        }
    }
}
