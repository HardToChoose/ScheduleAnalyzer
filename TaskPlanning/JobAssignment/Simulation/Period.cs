namespace TaskPlanning.JobAssignment.Simulation
{
    internal struct Period
    {
        public double From { get; set; }
        public double To { get; set; }

        public double Duration
        {
            get { return To - From; }
        }

        public static readonly Period Whole = new Period(0, double.MaxValue);

        public Period(double from, double to) : this()
        {
            From = from;
            To = to;
        }
    }
}
