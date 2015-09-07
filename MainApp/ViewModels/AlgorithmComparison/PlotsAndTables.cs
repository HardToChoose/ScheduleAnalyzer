namespace MainApp.ViewModels.AlgorithmComparison
{
    internal class PlotsAndTables
    {
        public PlotPack Plots { get; set; }

        public Table ExecutionTimes { get; set; }
        public Table PlanningTimes { get; set; }
        public Table Efficiency { get; set; }
        public Table SpeedUp { get; set; }
    }
}
