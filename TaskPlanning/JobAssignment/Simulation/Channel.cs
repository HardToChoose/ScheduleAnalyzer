using GraphLogic.Entities;

namespace TaskPlanning.JobAssignment.Simulation
{
    internal struct Channel
    {
        public Processor Source { get; set; }
        public Processor Target { get; set; }
        public double TransferSpeed { get; set; }

        public Channel Reverse()
        {
            return new Channel
            {
                Source = Target,
                Target = Source,
                TransferSpeed = TransferSpeed
            };
        }
    }
}
