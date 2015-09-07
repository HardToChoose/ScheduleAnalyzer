
namespace TaskPlanning.JobAssignment.Simulation.Events
{
    public abstract class TimelinePeriodBase
    {
        public double StartTime { get; set;  }
        public double Duration { get; set; }

        public double FinishTime
        {
            get { return StartTime + Duration; }
        }
    }
}
