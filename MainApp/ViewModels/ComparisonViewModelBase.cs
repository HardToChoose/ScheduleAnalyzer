using GraphLogic.Entities;
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
    internal abstract class ComparisonViewModelBase : BindableBase
    {
        #region Binding properties

        private DataSelector chosenDataSelector;

        private bool showTables;
        private bool showBestOnly;

        private bool isProgressVisible;
        private int maxProgress;
        private int progress;

        public DataSelector ChosenDataSelector
        {
            get { return chosenDataSelector; }
            set
            {
                chosenDataSelector = value;

                OnPropertyChanged("ChosenDataSelector");
                InvalidateData();
            }
        }

        public bool ShowTables
        {
            get { return showTables; }
            set
            {
                showTables = value;

                OnPropertyChanged("ShowTables");
                InvalidateData();
            }
        }

        public bool ShowBestOnly
        {
            get { return showBestOnly; }
            set
            {
                showBestOnly = value;
                BestOnlySelection(value);

                OnPropertyChanged("ShowBestOnly");
                InvalidateData();
            }
        }

        public bool IsProgressVisible
        {
            get { return isProgressVisible; }
            set
            {
                isProgressVisible = value;
                OnPropertyChanged("IsProgressVisible");
            }
        }

        public int MaxProgress
        {
            get { return maxProgress; }
            set
            {
                maxProgress = value;
                OnPropertyChanged("MaxProgress");
            }
        }

        public int Progress
        {
            get { return progress; }
            set
            {
                progress = value;
                OnPropertyChanged("Progress");
            }
        }

        public DataSelector[] DataSelectors { get; protected set; }

        #endregion

        #region Commands

        public DelegateCommand WindowLoadedCommand { get; protected set; }

        #endregion

        public PlotModel OxyPlotModel { get; protected set; }

        protected abstract void UpdatePlots();
        protected abstract void BestOnlySelection(bool selected);

        public abstract IEnumerable<object> Tables { get; }

        public ComparisonViewModelBase()
        {
            OxyPlotModel = new PlotModel
            {
                LegendFontSize = 13,
                LegendFont = "Consolas",
                LegendPadding = 12,
                LegendSymbolLength = 24,
                SelectionColor = OxyColors.Black
            };

            DataSelectors = new[]
            {
                new DataSelector
                {
                    Name = "час виконання",
                    SelectPlot = (arg, jobCount) => arg.Plots.GanntTimes[jobCount],
                    SelectTable = (arg) => arg.ExecutionTimes
                },

                new DataSelector
                {
                    Name = "коефіцієнт прискорення",
                    SelectPlot = (arg, jobCount) => arg.Plots.SpeedUp[jobCount],
                    SelectTable = (arg) => arg.SpeedUp
                },

                new DataSelector
                {
                    Name = "коефіцієнт ефективності",
                    SelectPlot = (arg, jobCount) => arg.Plots.Efficiency[jobCount],
                    SelectTable = (arg) => arg.Efficiency
                },

                new DataSelector
                {
                    Name = "час роботи алгоритмів",
                    SelectPlot = (arg, jobCount) => arg.Plots.AlgorithmExecutionTimes[jobCount],
                    SelectTable = (arg) => arg.PlanningTimes
                }
            };

            ShowTables = false;
            ChosenDataSelector = DataSelectors.First();

            Progress = 0;
            MaxProgress = 0;
            IsProgressVisible = true;
        }

        #region Protected methods

        protected void InvalidateData()
        {
            if (ShowTables)
                OnPropertyChanged("Tables");
            else
                UpdatePlots();
        }

        protected bool IsBetter(TestResults results, TestResults best, double bestCompResult, out double comparisonResult)
        {
            var betterByJobCount = results.Efficiency.Keys
                .Select(jobCount => CompareArrays(results.Efficiency[jobCount], best.Efficiency[jobCount]))
                .ToArray();

            comparisonResult = 0;
            return betterByJobCount.All(cmp => cmp >= 0) && (betterByJobCount.Sum() != 0);
        }

        protected int CompareArrays(double[] first, double[] second)
        {
            int result = 0;
            for (int k = 0; k < first.Length; k++)
            {
                if (first[k] > second[k])
                    result++;
                if (first[k] < second[k])
                    result--;
            }
            return result;
        }

        #endregion
    }
}
