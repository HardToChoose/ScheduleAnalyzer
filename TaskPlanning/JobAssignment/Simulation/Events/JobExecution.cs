using GraphLogic.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskPlanning.Queueing.Algorithms;

namespace TaskPlanning.JobAssignment.Simulation.Events
{
    public class JobExecution : TimelinePeriodBase
    {
        public Operation Job { get; set; }
    }
}
