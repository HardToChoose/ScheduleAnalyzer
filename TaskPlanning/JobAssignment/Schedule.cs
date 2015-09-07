using GraphLogic.Entities;

using System;
using System.Linq;
using System.Collections.Generic;

using TaskPlanning.JobAssignment.Simulation.Events;
using System.Diagnostics;

namespace TaskPlanning.JobAssignment
{
    public partial class Schedule : ICloneable
    {
        private Dictionary<Processor, List<TimelinePeriodBase>> timelines = new Dictionary<Processor, List<TimelinePeriodBase>>();

        internal void AddEvent(Processor processor, TimelinePeriodBase e)
        {
            var timeline = timelines[processor];
            var index = timeline.FindIndex(period => e.StartTime <= period.StartTime);

            if (index == -1)
                timeline.Add(e);
            else
            {
                timeline.Insert(index, e);

                //if (timeline[index] is DataTransfer && timeline[index + 1] is DataTransfer)
                //    Debug.Assert(!AreDuplex(timeline[index] as DataTransfer, timeline[index + 1] as DataTransfer));
            }
        }

        public bool AreDuplex(DataTransfer first, DataTransfer second)
        {
            return (first.StartTime >= second.StartTime || first.FinishTime > second.StartTime) &&
                   (first.FromProc == second.ToProc) && (first.ToProc == second.FromProc);
        }

        public Schedule(IEnumerable<Processor> pcs)
        {
            foreach (var proc in pcs)
                timelines[proc] = new List<TimelinePeriodBase>();
        }

        public IReadOnlyList<TimelinePeriodBase> this[Processor proc]
        {
            get { return timelines[proc]; }
        }

        public T LastEvent<T>() where T : TimelinePeriodBase
        {
            return timelines.SelectMany(pair => pair.Value).OrderBy(e => e.FinishTime).LastOrDefault(e => e is T) as T;
        }

        public double Duration
        {
            get
            {
                return timelines.SelectMany(pair => pair.Value)
                                .OrderBy(e => e.FinishTime)
                                .Last().FinishTime;
            }
        }

        public object Clone()
        {
            var result = new Schedule(timelines.Keys);
            foreach (var pair in timelines)
            {
                result.timelines[pair.Key] = new List<TimelinePeriodBase>(pair.Value);
            }
            return result;
        }
    }
}
