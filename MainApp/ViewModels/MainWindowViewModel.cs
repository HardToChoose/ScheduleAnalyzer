using GraphLogic.Graphs;
using GraphLogic.Entities;

using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Interactivity.InteractionRequest;
using Microsoft.Practices.Prism.Mvvm;
using Microsoft.Practices.Prism.PubSubEvents;

using QuickGraph;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

using TaskPlanning.Queueing.Algorithms;
using TaskPlanning.JobAssignment;
using TaskPlanning.JobAssignment.Algorithms;
using TaskPlanning.Queueing.Statistics;
using TaskPlanning.Queueing;

using Measuring;
using MainApp.DialogServices;

namespace MainApp.ViewModels
{
    class MainWindowViewModel : BindableBase
    {
        private static readonly IQueueingAlgorithm[] queueingAlgorithms;
        private static readonly JobAssignmentAlgorithm[] jobAssignmentAlgorithms;

        private static T[] CreateAllSubClassInstances<T>()
        {
            var baseType = typeof(T);
            return AppDomain.CurrentDomain.GetAssemblies()
                                          .SelectMany(ass => ass.GetTypes())
                                          .Where(qa => qa.IsClass && baseType.IsAssignableFrom(qa) &&
                                                          qa.GetConstructor(Type.EmptyTypes) != null)
                                          .Select(type => Activator.CreateInstance(type))
                                          .OfType<T>()
                                          .ToArray();
        }

        static MainWindowViewModel()
        {
            queueingAlgorithms = CreateAllSubClassInstances<IQueueingAlgorithm>();
            jobAssignmentAlgorithms = CreateAllSubClassInstances<JobAssignmentAlgorithm>();
        }

        private IDialogService statisticOptions = new StatisticOptionsDialogService();

        #region Binding properties

        private Schedule schedule;
        private PcsOptions pcsInfo;

        private TaskGraphStat pathInfo;
        private OperationQueue planningQueue;

        private IQueueingAlgorithm selectedQueueingAlgorithm;
        private JobAssignmentAlgorithm selectedJobAssignmentAlgorithm;

        private UndirectedWeightedGraph processorGraph;
        private DirectedWeightedGraph taskGraph;

        public PcsOptions PcsInfo
        {
            get
            {
                return pcsInfo;
            }
            set
            {
                pcsInfo = value;
                OnPropertyChanged("PcsInfo");
            }
        }

        public Schedule Schedule
        {
            get
            {
                return schedule;
            }
            set
            {
                schedule = value;
                OnPropertyChanged("Schedule");
            }
        }

        public IQueueingAlgorithm[] QueueingAlgorithms
        {
            get { return queueingAlgorithms; }
        }

        public JobAssignmentAlgorithm[] JobAssignmentAlgorithms
        {
            get { return jobAssignmentAlgorithms; }
        }

        public IQueueingAlgorithm SelectedQueueingAlgorithm
        {
            get
            {
                return selectedQueueingAlgorithm;
            }
            set
            {
                selectedQueueingAlgorithm = value;
                OnPropertyChanged("SelectedQueueingAlgorithm");

                MakeQueueCommand.RaiseCanExecuteChanged();
            }
        }

        public JobAssignmentAlgorithm SelectedJobAssignmentAlgorithm
        {
            get
            {
                return selectedJobAssignmentAlgorithm;
            }
            set
            {
                selectedJobAssignmentAlgorithm = value;
                OnPropertyChanged("SelectedJobAssignmentAlgorithm");

                AssignJobsCommand.RaiseCanExecuteChanged();
            }
        }

        public TaskGraphStat PathInfo
        {
            get
            {
                return pathInfo;
            }
            set
            {
                pathInfo = value;
                OnPropertyChanged("PathInfo");
            }
        }

        public OperationQueue PlanningQueue
        {
            get
            {
                return planningQueue;
            }
            set
            {
                planningQueue = value;
                OnPropertyChanged("PlanningQueue");

                AssignJobsCommand.RaiseCanExecuteChanged();
            }
        }

        public UndirectedWeightedGraph ProcessorGraph
        {
            get
            {
                return processorGraph;
            }
            set
            {
                processorGraph = value;
                OnPropertyChanged("ProcessorGraph");

                AssignJobsCommand.RaiseCanExecuteChanged();
            }
        }

        public DirectedWeightedGraph TaskGraph
        {
            get
            {
                return taskGraph;
            }
            set
            {
                taskGraph = value;
                OnPropertyChanged("TaskGraph");
                
                MakeQueueCommand.RaiseCanExecuteChanged();
                AssignJobsCommand.RaiseCanExecuteChanged();
            }
        }

        #endregion

        #region Interaction requests & commands

        public DelegateCommand MakeQueueCommand { get; private set; }
        public DelegateCommand AssignJobsCommand { get; private set; }
        public DelegateCommand GenerateTaskCommand { get; private set; }
        public DelegateCommand CompareAlgorithmsCommand { get; private set; }

        public InteractionRequest<Confirmation> ShowDialogRequest { get; private set; }
        public InteractionRequest<Confirmation> ShowFloatingWindowRequest { get; private set; }

        #endregion

        public MainWindowViewModel()
        {
            ShowDialogRequest = new InteractionRequest<Confirmation>();
            ShowFloatingWindowRequest = new InteractionRequest<Confirmation>();

            //GenerateTaskCommand = new DelegateCommand(ShowGraphGenerationDialog);
            AssignJobsCommand = new DelegateCommand(AssignJobs, CanAssignJobs);
            MakeQueueCommand = new DelegateCommand(MakeQueue, CanMakeQueue);
            //CompareAlgorithmsCommand = new DelegateCommand(CompareAlgorithms);

            ProcessorGraph = new UndirectedWeightedGraph();
            TaskGraph = new DirectedWeightedGraph();
        }

        public bool CanAssignJobs()
        {
            return !(TaskGraph == null ||
                     PlanningQueue == null ||
                     ProcessorGraph == null ||
                     SelectedJobAssignmentAlgorithm == null);
        }

        public void AssignJobs()
        {
            var algo = SelectedJobAssignmentAlgorithm;
            algo.Initialize(ProcessorGraph, TaskGraph, PlanningQueue);
            Schedule = algo.Compute(PcsInfo);
        }

        private bool CanMakeQueue()
        {
            return !(TaskGraph == null ||
                     SelectedQueueingAlgorithm == null);                     
        }

        public void MakeQueue()
        {
            if (SelectedQueueingAlgorithm != null && !TaskGraph.IsVerticesEmpty)
            {
                PathInfo = TaskGraphStat.Gather(TaskGraph);
                PlanningQueue = SelectedQueueingAlgorithm.Compute(TaskGraph.Vertices.Cast<Operation>(), PathInfo);
            }
        }

        private bool CanCompareAlgorithms()
        {
            return (processorGraph != null) &&
                   (processorGraph.VertexCount > 0);
        }
    }
}
