using System.Collections.Generic;

namespace Measuring
{
    public class TestResults
    {
        public Dictionary<int, double[]> AlgorithmExecutionTimes { get; private set; }

        public Dictionary<int, double[]> GanntTimes { get; private set; }
        public Dictionary<int, double[]> Efficiency { get; private set; }
        public Dictionary<int, double[]> SpeedUp { get; private set; }

        internal TestResults()
        {
            AlgorithmExecutionTimes = new Dictionary<int, double[]>();

            GanntTimes = new Dictionary<int, double[]>();
            Efficiency = new Dictionary<int, double[]>();
            SpeedUp = new Dictionary<int, double[]>();
        }
    }
}
