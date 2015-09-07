using GraphLogic.Entities;
using GraphLogic.Random;

using MainApp.DialogServices;
using MainApp.ViewModels;
using UI.Helpers;

using Microsoft.Practices.Prism.Mvvm;

using System.Linq;
using System.Windows;

using Measuring;
using TaskPlanning.JobAssignment;

namespace MainApp.Views
{
    public partial class MainWindow : Window, IView
    {
        private PathInfoFloatingWindow pathInfoWindow = new PathInfoFloatingWindow();
        private QueueingFloatingWindow queueWindow = new QueueingFloatingWindow();
        
        public MainWindow()
        {
            InitializeComponent();
            Loaded += delegate
            {
                pathInfoWindow.Owner = this;
                queueWindow.Owner = this;
            };
        }

        private void ShowFloating(Window window, object dataContext)
        {
            window.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            window.DataContext = dataContext;
            window.Show();
        }

        private void AssignJobsButton_Click(object sender, RoutedEventArgs e)
        {
            var vm = this.DataContext as MainWindowViewModel;
            var options = new PcsOptions(vm.ProcessorGraph.Vertices.Cast<Processor>());

            var dialog = new JobAssignmentDialog
            {
                Owner = this,
                DataContext = options
            };
            
            if (dialog.ShowDialog() == true && vm.CanAssignJobs())
            {
                vm.PcsInfo = options;
                vm.AssignJobs();

                var window = new GanntChartWindow(options, vm.Schedule)
                {
                    Owner = this,
                    DataContext = this.DataContext
                };
                window.Show();
            }
        }

        private void GenerateTaskGraphButton_Click(object sender, RoutedEventArgs e)
        {
            var vm = this.DataContext as MainWindowViewModel;
            var taskGenerationInfo = new TaskGenerationInfo();

            var dialog = new TaskGenerationDialog
            {
                Owner = this,
                DataContext = taskGenerationInfo
            };

            if (dialog.ShowDialog() == true)
            {
                vm.TaskGraph = RandomGraph.GenerateTask(taskGenerationInfo);
            }            
        }

        private void MakeQueueButton_Click(object sender, RoutedEventArgs e)
        {
            if (MakeQueueButton.Command != null && MakeQueueButton.Command.CanExecute(null))
            {
                MakeQueueButton.Command.Execute(MakeQueueButton.CommandParameter);

                ShowFloating(pathInfoWindow, (DataContext as dynamic).PathInfo);
                ShowFloating(queueWindow, (DataContext as dynamic).PlanningQueue);

                if (ActualWidth < CurrentScreen.Width)
                {
                    this.PlaceFloatingWindow(pathInfoWindow, new PlaceInfo { AttachTo = AttachPosition.Left, OutsideHorizontal = true });
                    this.PlaceFloatingWindow(queueWindow, new PlaceInfo { AttachTo = AttachPosition.Right, OutsideHorizontal = true });
                }
                else
                {
                    this.PlaceFloatingWindow(pathInfoWindow, new PlaceInfo { AttachTo = AttachPosition.Left, SkipParentBorder = true, OffsetX = 5 });
                    this.PlaceFloatingWindow(queueWindow, new PlaceInfo { AttachTo = AttachPosition.Right, SkipParentBorder = true, OffsetX = -5 });
                }
            }
        }

        private IDialogService testOptionsDialog = new StatisticOptionsDialogService();

        private void CompareAlgorithms_Click(object sender, RoutedEventArgs e)
        {
            var main = this.DataContext as MainWindowViewModel;
            var testOptions = new TestOptionsDialogViewModel
            {
                JobCount = new IntegerRange
                {
                    From = main.ProcessorGraph.VertexCount,
                    Step = main.ProcessorGraph.VertexCount,
                    To = main.ProcessorGraph.VertexCount * 4
                }
            };

            if (testOptionsDialog.ShowDialog(this, testOptions) == true)
            {
                new AlgorithmComparisonWindow
                {
                    Owner = this,
                    DataContext = new AlgorithmComparisonViewModel(main, testOptions),
                    WindowStartupLocation = WindowStartupLocation.CenterOwner
                }
                .Show();
            }
        }
    }
}
