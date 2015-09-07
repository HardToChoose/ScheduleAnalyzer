using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskPlanning.JobAssignment.Simulation.Events;

namespace TaskPlanning.JobAssignment.Simulation.Helpers
{
    internal class FreeTimeFinder
    {
        public static double FindFirstFreePeriod(double timeNow, double duration, List<TimelinePeriodBase> events)
        {
            return GetFreePeriods(timeNow, events).First(period => period.Duration >= duration).From;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="timeNow"></param>
        /// <param name="events">A sorted list of events</param>
        /// <returns></returns>
        public static IReadOnlyList<Period> GetFreePeriods(double timeNow, IReadOnlyList<TimelinePeriodBase> events)
        {
            if (events.Count == 0)
                return new[] { new Period(timeNow, double.PositiveInfinity) };

            if (timeNow >= events.Last().StartTime)
                return new[] { new Period(Math.Max(timeNow, events.Last().FinishTime), double.PositiveInfinity) };

            int k = 0;
            for (; events[k].StartTime <= timeNow; k++) ;

            var result = new List<Period>
            {
                new Period((k == 0) ? timeNow : Math.Max(timeNow, events[k - 1].FinishTime), events[k].StartTime)
            };

            for (; k < events.Count - 1; k++)
            {
                result.Add(new Period(events[k].FinishTime, events[k + 1].StartTime));
            }

            result.Add(new Period(events.Last().FinishTime, double.MaxValue));
            return result;
        }

        public static IReadOnlyList<Period> FreePeriodsIntersection(IReadOnlyList<Period> periods_1, IReadOnlyList<Period> periods_2)
        {
            var result = new List<Period>();

            foreach (var a in periods_1)
            {
                foreach (var b in periods_2)
                {
                    var diffStart = b.To - a.From;
                    var diffEnd = a.To - b.From;

                    /* Periods don't intersect at all */
                    if (diffStart <= 0 || diffEnd <= 0)
                        continue;

                    result.Add(new Period(Math.Max(a.From, b.From), Math.Min(a.To, b.To)));
                }
            }
            return result;
        }
    }
}
