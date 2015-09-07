using GraphLogic.Entities;
using GraphLogic.Graphs;

using MainApp.ViewModels.AlgorithmComparison;
using Measuring;

using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Mvvm;

using OxyPlot;
using OxyPlot.Series;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using TaskPlanning.JobAssignment;
using TaskPlanning.JobAssignment.Algorithms;
using TaskPlanning.Queueing.Algorithms;

namespace MainApp.ViewModels
{
    internal class AlgorithmComparisonViewModel : ComparisonViewModelBase
    {
        #region Binding properties

        public ListItem<int>[] JobCounts { get; private set; }
        public ListItem<IQueueingAlgorithm>[] QueueingAlgorithms { get; private set; }
        public ListItem<JobAssignmentAlgorithm>[] JobAssignmentAlgorithms { get; private set; }

        #endregion

        #region Private fields

        private Tuple<int, int> bestAlgorithmPair;
        private Dictionary<IQueueingAlgorithm, Dictionary<JobAssignmentAlgorithm, PlotsAndTables>> data;

        private double[] connectivities;
        private MainWindowViewModel main;
        public TestOptionsDialogViewModel TestOptions;

        private List<int> SelectedJobCounts = new List<int>();
        private List<IQueueingAlgorithm> SelectedQueueingAlgorithms = new List<IQueueingAlgorithm>();
        private List<JobAssignmentAlgorithm> SelectedAssignmentAlgorithms = new List<JobAssignmentAlgorithm>();

        public PcsOptions Options { get; private set; }
        public TestBench Bench { get; private set; }
        public UndirectedWeightedGraph Pcs { get; private set; }

        #endregion

        public Tuple<IQueueingAlgorithm, JobAssignmentAlgorithm> SelectedPlotAlgoPair
        {
            get
            {
                if (ShowBestOnly)
                    return new Tuple<IQueueingAlgorithm, JobAssignmentAlgorithm>(
                        QueueingAlgorithms[bestAlgorithmPair.Item1].Item,
                        JobAssignmentAlgorithms[bestAlgorithmPair.Item2].Item);

                var selected = OxyPlotModel.Series.First(plot => plot.IsSelected());
                foreach (var q in SelectedQueueingAlgorithms)
                    foreach (var p in SelectedAssignmentAlgorithms)
                        foreach (var selector in DataSelectors)
                            foreach (var j in SelectedJobCounts)
                                if (selector.SelectPlot(data[q][p], j) == selected)
                                    return new Tuple<IQueueingAlgorithm, JobAssignmentAlgorithm>(q, p);
                return null;
            }
        }

        public AlgorithmComparisonViewModel(MainWindowViewModel mainViewModel, TestOptionsDialogViewModel testOptions)
        {
            Pcs = mainViewModel.ProcessorGraph;
            TestOptions = testOptions;

            main = mainViewModel;
            connectivities = TestOptions.Connectivity.ToArray();

            bestAlgorithmPair = new Tuple<int, int>(0, 0);
            data = new Dictionary<IQueueingAlgorithm, Dictionary<JobAssignmentAlgorithm, PlotsAndTables>>();

            JobCounts = TestOptions.JobCount
                .Select(jobCount => new ListItem<int>(jobCount, jobCount.ToString(), JobCountSelection))
                .ToArray();
            QueueingAlgorithms = main.QueueingAlgorithms
                .Select(algo => new ListItem<IQueueingAlgorithm>(algo, algo.Name, QueueingAlgoSelection))
                .ToArray();
            JobAssignmentAlgorithms = main.JobAssignmentAlgorithms
                .Select(algo => new ListItem<JobAssignmentAlgorithm>(algo, algo.AlgorithmID.ToString(), PlanningAlgoSelection))
                .ToArray();

            Options = new PcsOptions(main.ProcessorGraph.Vertices.Cast<Processor>(), true, true);
            Bench = new TestBench(TestOptions.Connectivity, TestOptions.JobCount, TestOptions.JobComplexity, main.ProcessorGraph, TestOptions.TestRuns);

            WindowLoadedCommand = new DelegateCommand(() => Task.Factory.StartNew(DoAllPairsTests));
            //WindowLoadedCommand = new DelegateCommand(DoTests);
        }

        #region Private methods

        private void DoAllPairsTests()
        {   
            int q = 0;
            int p = 0;
            int subProgress = TestOptions.JobCount.ValueCount *
                                TestOptions.Connectivity.ValueCount *
                                  TestOptions.TestRuns;
            MaxProgress = main.QueueingAlgorithms.Length *
                            main.JobAssignmentAlgorithms.Length *
                              subProgress;
            Progress = 0;

            var progress = new Progress<int>(value => Progress = (q * main.JobAssignmentAlgorithms.Length + p) * subProgress + value);
            
            TestResults bestResults = null;
            double bestComparisonResult = 0.0;

            for (q = 0; q < main.QueueingAlgorithms.Length; q++)
            {
                var queueingAlgo = main.QueueingAlgorithms[q];
                data[queueingAlgo] = new Dictionary<JobAssignmentAlgorithm, PlotsAndTables>();

                for (p = 0; p < main.JobAssignmentAlgorithms.Length; p++)
                {
                    var planningAlgo = main.JobAssignmentAlgorithms[p];
                    var testResults = Bench.Run(queueingAlgo, planningAlgo, Options, progress);
                    var comparisonResult = 0.0;

                    if (bestResults == null || IsBetter(testResults, bestResults, bestComparisonResult, out comparisonResult))
                    {
                        bestResults = testResults;
                        bestAlgorithmPair = new Tuple<int,int>(q, p);
                        bestComparisonResult = comparisonResult;
                    }

                    var plotsAndTables = new PlotsAndTables
                    {
                        /* Convert each Dictionary<int, double[]> to Table */
                        ExecutionTimes = DictToTable(queueingAlgo, planningAlgo, testResults.GanntTimes),
                        SpeedUp = DictToTable(queueingAlgo, planningAlgo, testResults.SpeedUp),
                        Efficiency = DictToTable(queueingAlgo, planningAlgo, testResults.Efficiency),
                        PlanningTimes = DictToTable(queueingAlgo, planningAlgo, testResults.AlgorithmExecutionTimes),

                        /* Convert each Dictionary<int, double[]> to Dictionary<int, Series> */
                        Plots = new PlotPack
                        {
                            GanntTimes = DictToPlots(queueingAlgo, planningAlgo, testResults.GanntTimes),
                            SpeedUp = DictToPlots(queueingAlgo, planningAlgo, testResults.SpeedUp),
                            Efficiency = DictToPlots(queueingAlgo, planningAlgo, testResults.Efficiency),
                            AlgorithmExecutionTimes = DictToPlots(queueingAlgo, planningAlgo, testResults.AlgorithmExecutionTimes)
                        }
                    };
                    data[queueingAlgo][planningAlgo] = plotsAndTables;
                }
            }

            /* Show one plot */
            JobCounts.First().IsSelected = true;
            QueueingAlgorithms.First().IsSelected = true;
            JobAssignmentAlgorithms.First().IsSelected = true;

            IsProgressVisible = false;
        }

        private Table DictToTable(IQueueingAlgorithm queueingAlgo, JobAssignmentAlgorithm planningAlgo, Dictionary<int, double[]> dict)
        {
            return new Table
            {
                Title = string.Format("Алгоритми: {0} : {1}", queueingAlgo.Name, planningAlgo.AlgorithmID),
                JobCounts = SelectedJobCounts,
                Data = Enumerable.Range(0, connectivities.Length)
                                 .Select(rowIndex => TableRow.Create(dict, SelectedJobCounts, rowIndex))
            };
        }

        private Dictionary<int, Series> DictToPlots(IQueueingAlgorithm queueingAlgo, JobAssignmentAlgorithm planningAlgo, Dictionary<int, double[]> dict)
        {
            var result = new Dictionary<int, Series>();

            foreach (var jobCount in dict.Keys)
            {
                var plot = new LineSeries
                {
                    StrokeThickness = 2.4,
                    Title = string.Format(" {0,2} : {1} | {2}", queueingAlgo.Name, planningAlgo.AlgorithmID, jobCount)
                };

                if (planningAlgo.AlgorithmID == 2)
                {
                    plot.LineStyle = LineStyle.Dash;
                    plot.Dashes = new double[] { 4, 3 };
                    
                    if (queueingAlgo is VertWeightAsc)
                        plot.Color = OxyColor.FromRgb(240, 59, 32);
                    else if (queueingAlgo is VertPathVertRootDesc)
                        plot.Color = OxyColor.FromRgb(44, 127, 184);
                    else if (queueingAlgo is NormTimeVertSumDesc)
                        plot.Color = OxyColor.FromRgb(99, 99, 99);
                }
                else
                {
                    if (queueingAlgo is VertWeightAsc)
                        plot.Color = OxyColor.FromRgb(254, 178, 76);
                    else if (queueingAlgo is VertPathVertRootDesc)
                        plot.Color = OxyColor.FromRgb(127, 205, 187);
                    else if (queueingAlgo is NormTimeVertSumDesc)
                        plot.Color = OxyColor.FromRgb(189, 189, 189);
                }

                plot.MouseDown += (sender, args) =>
                {
                    if (args.ClickCount == 2)
                    {
                        var series = sender as LineSeries;
                        if (series.IsSelected())
                        {
                            series.StrokeThickness /= 1.5;
                            series.ClearSelection();

                            OxyPlotModel.InvalidatePlot(false);
                        }
                        else
                        {
                            series.StrokeThickness *= 1.5;
                            series.ClearSelection();
                            series.Select();

                            OxyPlotModel.InvalidatePlot(false);
                        }
                    }
                };

                for (int k = 0; k < connectivities.Length; k++)
                {
                    plot.Points.Add(new DataPoint(connectivities[k], dict[jobCount][k]));
                }
                result[jobCount] = plot;
            }
            return result;
        }

        #endregion

        #region Base class implementation

        public override IEnumerable<object> Tables
        {
            get
            {
                if (ShowBestOnly)
                {
                    var qAlgo = QueueingAlgorithms[bestAlgorithmPair.Item1].Item;
                    var pAlgo = JobAssignmentAlgorithms[bestAlgorithmPair.Item2].Item;

                    yield return ChosenDataSelector.SelectTable(data[qAlgo][pAlgo]);
                }
                else
                {
                    foreach (var qAlgo in SelectedQueueingAlgorithms)
                        foreach (var pAlgo in SelectedAssignmentAlgorithms)
                            yield return ChosenDataSelector.SelectTable(data[qAlgo][pAlgo]);
                }
            }
        }

        protected override void BestOnlySelection(bool selected)
        {
            if (!selected)
            {
                QueueingAlgorithms[bestAlgorithmPair.Item1].SelectUIOnly(false);
                JobAssignmentAlgorithms[bestAlgorithmPair.Item2].SelectUIOnly(false);
            }

            foreach (var q in QueueingAlgorithms.Where(item => SelectedQueueingAlgorithms.Contains(item.Item)))
            {
                q.SelectUIOnly(!selected);
            }
            foreach (var p in JobAssignmentAlgorithms.Where(item => SelectedAssignmentAlgorithms.Contains(item.Item)))
            {
                p.SelectUIOnly(!selected);
            }

            if (selected)
            {
                QueueingAlgorithms[bestAlgorithmPair.Item1].SelectUIOnly(true);
                JobAssignmentAlgorithms[bestAlgorithmPair.Item2].SelectUIOnly(true);
            }
        }

        protected override void UpdatePlots()
        {
            OxyPlotModel.Series.Clear();

            if (ShowBestOnly)
            {
                var q = QueueingAlgorithms[bestAlgorithmPair.Item1].Item;
                var p = JobAssignmentAlgorithms[bestAlgorithmPair.Item2].Item;

                foreach (var j in SelectedJobCounts)
                    OxyPlotModel.Series.Add(ChosenDataSelector.SelectPlot(data[q][p], j));
            }
            else
            {
                foreach (var q in SelectedQueueingAlgorithms)
                    foreach (var p in SelectedAssignmentAlgorithms)
                        foreach (var j in SelectedJobCounts)
                            OxyPlotModel.Series.Add(ChosenDataSelector.SelectPlot(data[q][p], j));
            }
            OxyPlotModel.InvalidatePlot(true);
        }

        #endregion

        #region Plot selection handlers

        private void QueueingAlgoSelection(IQueueingAlgorithm algo, bool isSelected)
        {
            if (isSelected)     SelectedQueueingAlgorithms.Add(algo);
            else                SelectedQueueingAlgorithms.Remove(algo);

            InvalidateData();
        }

        private void PlanningAlgoSelection(JobAssignmentAlgorithm algo, bool isSelected)
        {
            if (isSelected)     SelectedAssignmentAlgorithms.Add(algo);
            else                SelectedAssignmentAlgorithms.Remove(algo);

            InvalidateData();
        }

        private void JobCountSelection(int jobCount, bool isSelected)
        {
            if (isSelected)     SelectedJobCounts.Add(jobCount);
            else                SelectedJobCounts.Remove(jobCount);

            InvalidateData();
        }

        #endregion
    }
}
