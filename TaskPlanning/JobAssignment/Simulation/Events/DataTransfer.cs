using GraphLogic.Entities;
using System.Diagnostics;
using TaskPlanning.Queueing.Algorithms;

namespace TaskPlanning.JobAssignment.Simulation.Events
{
    [DebuggerDisplay("{StartTime}..{FinishTime}: ({FromProc.ID}.{FromProcChannel}){FromOperation.ID}-{ToOperation.ID}({ToProc.ID}.{ToProcChannel})")]
    public class DataTransfer : TimelinePeriodBase
    {
        public double DataSize { get; set; }

        public Processor FromProc { get; set; }
        public Processor ToProc { get; set; }

        public int FromProcChannel { get; set; }
        public int ToProcChannel { get; set; }

        public Operation FromOperation { get; set; }
        public Operation ToOperation { get; set; }

        public bool UsesChannel(Processor proc, int channel)
        {
            return (proc == FromProc && FromProcChannel == channel) ||
                   (proc == ToProc && ToProcChannel == channel);
        }

        public bool IsBetween(Processor a, Processor b)
        {
            return (FromProc == a && ToProc == b) || (ToProc == a && FromProc == b);
        }

        public bool CanTransferDuplex(Processor from, Processor to)
        {
            return (FromProc == to && ToProc == from);
        }
    }
}
