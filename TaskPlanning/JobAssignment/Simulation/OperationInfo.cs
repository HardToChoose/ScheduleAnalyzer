using GraphLogic.Entities;

namespace TaskPlanning.JobAssignment.Simulation
{
    public class OperationInfo
    {
        public double StartTime { get; set; }
        public double FinishTime { get; set; }

        public OperationState State { get; set; }
        public Processor AssignedProc { get; set; }

        public OperationInfo()
        {
            AssignedProc = null;
            State = OperationState.NotReady;

            StartTime = FinishTime = 0.0;
        }
    }
}
