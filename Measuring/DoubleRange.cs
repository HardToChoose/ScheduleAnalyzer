using System.Collections.Generic;

namespace Measuring
{
    public class DoubleRange : RangeBase<double>
    {
        public override int ValueCount
        {
            get { return (int)(1 + (To - From) / Step); }
        }

        public override IEnumerator<double> GetEnumerator()
        {
            int k = 0;
            int total = ValueCount - 1;

            for (double current = From; k < total; current += Step, k++)
            {
                yield return current;
            }
            yield return To;
        }
    }
}
