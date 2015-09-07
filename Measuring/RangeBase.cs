using Entities;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace Measuring
{
    public abstract class RangeBase<T> : BindableBase, IEnumerable<T>
    {
        private T from;
        private T to;
        private T step;

        public T From
        {
            get
            {
                return from;
            }
            set
            {
                from = value;
                RaisePropertyChanged("From");
            }
        }

        public T To
        {
            get
            {
                return to;
            }
            set
            {
                to = value;
                RaisePropertyChanged("To");
            }
        }

        public T Step
        {
            get
            {
                return step;
            }
            set
            {
                step = value;
                RaisePropertyChanged("Step");
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public abstract int ValueCount { get; }
        public abstract IEnumerator<T> GetEnumerator();
    }
}
