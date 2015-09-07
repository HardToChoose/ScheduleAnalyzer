using GraphLogic.Graphs;
using Measuring;

using Microsoft.Practices.Prism.Commands;
using MainApp.ViewModels.AlgorithmComparison;

using OxyPlot;
using OxyPlot.Series;

using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

using TaskPlanning.JobAssignment;
using TaskPlanning.JobAssignment.Algorithms;
using TaskPlanning.Queueing.Algorithms;

namespace MainApp.ViewModels
{
    internal class PcsComparisonViewModel : ComparisonViewModelBase
    {
        #region Binding properties

        private bool? ioBackup;
        private bool? duplexBackup;

        private bool? duplexEnabled;
        private bool? ioControllersPresent;

        public bool? DuplexEnabled
        {
            get { return duplexEnabled; }
            set
            {
                duplexEnabled = value;

                OnPropertyChanged("DuplexEnabled");
                InvalidateData();
            }
        }

        public bool? IoControllersPresent
        {
            get { return ioControllersPresent; }
            set
            {
                ioControllersPresent = value;

                OnPropertyChanged("IoControllersPresent");
                InvalidateData();
            }
        }

        public ListItem<int>[] JobCounts { get; private set; }
        public ListItem<int>[] PhysicalLinks { get; private set; }

        #endregion

        #region Private fields

        private Tuple<bool, bool, int> bestConfig;
        private Dictionary<int, Dictionary<int, PlotsAndTables>> data;

        private double[] connectivities;
        private AlgorithmComparisonViewModel parentVM;
        private TestOptionsDialogViewModel testOptionsVM;

        private List<int> SelectedJobCounts = new List<int>();
        private List<int> SelectedPhysicalLinks = new List<int>();

        private IQueueingAlgorithm queueingAlgo;
        private JobAssignmentAlgorithm planningAlgo;

        private int maxPhysicalLinks;
        private UndirectedWeightedGraph pcs;

        #endregion

        public PcsComparisonViewModel(AlgorithmComparisonViewModel parent, TestOptionsDialogViewModel testOptions, IQueueingAlgorithm qAlgo, JobAssignmentAlgorithm pAlgo)
        {
            queueingAlgo = qAlgo;
            planningAlgo = pAlgo;

            parentVM = parent;
            testOptionsVM = testOptions;
            connectivities = testOptions.Connectivity.ToArray();

            pcs = parentVM.Pcs;
            maxPhysicalLinks = pcs.Vertices.Max(vertex => pcs.AdjacentDegree(vertex));

            bestConfig = new Tuple<bool, bool, int>(false, false, 0);
            data = new Dictionary<int, Dictionary<int, PlotsAndTables>>();

            JobCounts = testOptions.JobCount
                .Select(jobCount => new ListItem<int>(jobCount, jobCount.ToString(), JobCountSelection))
                .ToArray();

            PhysicalLinks = Enumerable.Range(1, maxPhysicalLinks)
                .Select(physLinks => new ListItem<int>(physLinks, physLinks.ToString(), PhysicalLinkSelection))
                .ToArray();

            WindowLoadedCommand = new DelegateCommand(() => Task.Factory.StartNew(DoBestPairTests));
            //WindowLoadedCommand = new DelegateCommand(DoBestPairTests);
        }

        #region Private methods

        private void DoBestPairTests()
        {
            var ioControllerAvail = new[] { false, true };
            var duplexModeAvail = new[] { false, true };

            int i = 0;
            int d = 0;
            int p = 0;
            int subProgress = testOptionsVM.JobCount.ValueCount *
                                testOptionsVM.Connectivity.ValueCount *
                                  testOptionsVM.TestRuns;

            MaxProgress = 2 * 2 * maxPhysicalLinks * subProgress;
            Progress = 0;

            var progress = new Progress<int>(value => Progress = ((i * 2 + d) * maxPhysicalLinks + p) * subProgress + value);

            TestResults bestResults = null;
            double bestComparisonResult = 0.0;

            PcsOptions options = parentVM.Options;
            TestBench test = parentVM.Bench;

            for (i = 0; i < 2; i++)
            {
                var hasIoControllers = ioControllerAvail[i];

                for (d = 0; d < 2; d++)
                {
                    var hasDuplex = duplexModeAvail[d];
                    data[i * 2 + d] = new Dictionary<int, PlotsAndTables>();

                    for (p = 0; p < maxPhysicalLinks; p++)
                    {
                        options.SetForAll(p + 1);
                        options.DuplexTransfer = hasDuplex;
                        options.IoControllerPresent = hasIoControllers;

                        var testResults = test.Run(queueingAlgo, planningAlgo, options, progress);
                        var comparisonResult = 0.0;

                        if (bestResults == null || IsBetter(testResults, bestResults, bestComparisonResult, out comparisonResult))
                        {
                            bestResults = testResults;
                            bestConfig = new Tuple<bool, bool, int>(hasIoControllers, hasDuplex, p + 1);
                            bestComparisonResult = comparisonResult;
                        }

                        var plotsAndTables = new PlotsAndTables
                        {
                            /* Convert each Dictionary<int, double[]> to Table */
                            ExecutionTimes = DictToTable(hasIoControllers, hasDuplex, p + 1, testResults.GanntTimes),
                            SpeedUp = DictToTable(hasIoControllers, hasDuplex, p + 1, testResults.SpeedUp),
                            Efficiency = DictToTable(hasIoControllers, hasDuplex, p + 1, testResults.Efficiency),
                            PlanningTimes = DictToTable(hasIoControllers, hasDuplex, p + 1, testResults.AlgorithmExecutionTimes),

                            /* Convert each Dictionary<int, double[]> to Dictionary<int, Series> */
                            Plots = new PlotPack
                            {
                                GanntTimes = DictToPlots(hasIoControllers, hasDuplex, p + 1, testResults.GanntTimes),
                                SpeedUp = DictToPlots(hasIoControllers, hasDuplex, p + 1, testResults.SpeedUp),
                                Efficiency = DictToPlots(hasIoControllers, hasDuplex, p + 1, testResults.Efficiency),
                                AlgorithmExecutionTimes = DictToPlots(hasIoControllers, hasDuplex, p + 1, testResults.AlgorithmExecutionTimes)
                            }
                        };
                        data[i * 2 + d][p + 1] = plotsAndTables;
                    }
                }
            }

            /* Show one plot */
            JobCounts.First().IsSelected = true;
            PhysicalLinks.First().IsSelected = true;

            IoControllersPresent = false;
            DuplexEnabled = false;

            IsProgressVisible = false;
        }

        private IEnumerable<int> StatesToIndicies()
        {
            var io = (IoControllersPresent == null) ? new[] { false, true } : new[] { IoControllersPresent.Value };
            var duplex = (DuplexEnabled == null) ? new[] { false, true } : new[] { DuplexEnabled.Value };

            foreach (var i in io)
                foreach (var d in duplex)
                    yield return ModesToIndex(i, d);
        }

        private int ModesToIndex(bool io, bool duplex)
        {
            return (io ? 1 : 0) * 2 + (duplex ? 1 : 0);
        }

        private string ConfigShortand(int physicalLinks, bool io, bool duplex, int jobCount = -1)
        {
            var modes = new List<string>(2);

            if (io)     modes.Add("io");
            if (duplex) modes.Add("duplex");
            
            if (jobCount == -1)
                return string.Format("{0} | {1}", physicalLinks, string.Join(", ", modes));
            else
                return string.Format(" {0}[{1,2}] | {2}", physicalLinks, jobCount, string.Join(", ", modes));
        }

        private Table DictToTable(bool io, bool duplex, int physicalLinks, Dictionary<int, double[]> dict)
        {
            return new Table
            {
                Title = "Фізичних каналів: " + ConfigShortand(physicalLinks, io, duplex),
                JobCounts = SelectedJobCounts,
                Data = Enumerable.Range(0, connectivities.Length)
                                 .Select(rowIndex => TableRow.Create(dict, SelectedJobCounts, rowIndex))
            };
        }

        private Dictionary<int, Series> DictToPlots(bool io, bool duplex, int physicalLinks, Dictionary<int, double[]> dict)
        {
            var result = new Dictionary<int, Series>();

            foreach (var jobCount in dict.Keys)
            {
                var plot = new LineSeries
                {
                    StrokeThickness = 2.4,
                    Title = ConfigShortand(physicalLinks, io, duplex, jobCount)
                };

                if (duplex)
                {
                    plot.LineStyle = LineStyle.Dash;
                    plot.Dashes = new double[] { 4, 3 };
                }

                if (io)
                {
                    switch (physicalLinks)
                    {
                        case 1: plot.Color = OxyColor.FromRgb(240, 59, 32); break;
                        case 2: plot.Color = OxyColor.FromRgb(44, 127, 184); break;
                        case 3: plot.Color = OxyColor.FromRgb(49, 163, 84); break;
                        case 4: plot.Color = OxyColor.FromRgb(221, 28, 119); break;
                    }
                }
                else
                {
                    switch (physicalLinks)
                    {
                        case 1: plot.Color = OxyColor.FromRgb(254, 178, 76); break;
                        case 2: plot.Color = OxyColor.FromRgb(127, 205, 187); break;
                        case 3: plot.Color = OxyColor.FromRgb(173, 221, 142); break;
                        case 4: plot.Color = OxyColor.FromRgb(201, 148, 199); break;
                    }
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
                    var index = ModesToIndex(bestConfig.Item1, bestConfig.Item2);
                    var physLinks = bestConfig.Item3;

                    yield return ChosenDataSelector.SelectTable(data[index][physLinks]);
                }
                else
                {
                    foreach (var index in StatesToIndicies())
                        foreach (var pLinks in SelectedPhysicalLinks)
                            yield return ChosenDataSelector.SelectTable(data[index][pLinks]);
                }
            }
        }

        protected override void BestOnlySelection(bool selected)
        {
            if (selected)
            {
                ioBackup = IoControllersPresent;
                duplexBackup = DuplexEnabled;

                IoControllersPresent = bestConfig.Item1;
                DuplexEnabled = bestConfig.Item2;
            }
            else
            {
                IoControllersPresent = ioBackup;
                DuplexEnabled = duplexBackup;
            }
        }

        protected override void UpdatePlots()
        {
            OxyPlotModel.Series.Clear();

            if (ShowBestOnly)
            {
                var index = ModesToIndex(bestConfig.Item1, bestConfig.Item2);
                var physLinks = bestConfig.Item3;

                foreach (var j in SelectedJobCounts)
                    OxyPlotModel.Series.Add(ChosenDataSelector.SelectPlot(data[index][physLinks], j));
            }
            else
            {
                foreach (var index in StatesToIndicies())
                    foreach (var physLinks in SelectedPhysicalLinks)
                        foreach (var j in SelectedJobCounts)
                            OxyPlotModel.Series.Add(ChosenDataSelector.SelectPlot(data[index][physLinks], j));
            }
            OxyPlotModel.InvalidatePlot(true);
        }

        #endregion

        #region Plot selection handlers

        private void PhysicalLinkSelection(int physicalLinks, bool isSelected)
        {
            if (isSelected)     SelectedPhysicalLinks.Add(physicalLinks);
            else                SelectedPhysicalLinks.Remove(physicalLinks);

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
