using System.Collections.Generic;

namespace Measuring
{
    public class IntegerRange : RangeBase<int>
    {
        public override int ValueCount
        {
            get { return 1 + (To - From) / Step; }
        }

        public override IEnumerator<int> GetEnumerator()
        {
            for (int current = From; current <= To; current += Step)
            {
                yield return current;
            }
        }
    }
}
