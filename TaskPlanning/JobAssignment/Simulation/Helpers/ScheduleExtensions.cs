using GraphLogic.Entities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskPlanning.JobAssignment;
using TaskPlanning.JobAssignment.Simulation.Events;
using TaskPlanning.JobAssignment.Simulation.Helpers;

namespace TaskPlanning.JobAssignment
{
    public partial class Schedule
    {
        internal double GetProcessorIdleTime(Processor proc, double timeNow, bool hasIOc)
        {
            var events = (hasIOc ? timelines[proc].Where(e => e is JobExecution) : timelines[proc]).ToArray();
            if (events.Length == 0)
                return timeNow;

            var result = timeNow - events.Last().FinishTime;
            Debug.Assert(result >= 0);
            return result;
        }

        internal int GetFreeChannel(Processor proc, PcsOptions options, double timeNow, bool duplex)
        {
            /* Get the number of busy proc channels and a given time */
            var transfers = timelines[proc].OfType<DataTransfer>();
            return 0;
        }

        internal bool IsProcessorFree(Processor proc, double timeNow, bool hasIOc)
        {
            TimelinePeriodBase last = hasIOc ?
                timelines[proc].OfType<JobExecution>().LastOrDefault() :
                timelines[proc].LastOrDefault();

            return (last == null) || (last.StartTime < last.FinishTime && last.FinishTime <= timeNow);
        }
    }
}
