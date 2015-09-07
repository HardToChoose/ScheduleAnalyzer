using Measuring;
using Microsoft.Practices.Prism.Mvvm;

namespace MainApp.ViewModels
{
    class TestOptionsDialogViewModel : BindableBase
    {
        #region Binding properties

        private IntegerRange jobCount;
        private IntegerRange jobComplexity;
        private DoubleRange connectivity;
        private int testRuns;

        public IntegerRange JobCount
        {
            get
            {
                return jobCount;
            }
            set
            {
                jobCount = value;
                OnPropertyChanged("JobCount");
            }
        }

        public IntegerRange JobComplexity
        {
            get
            {
                return jobComplexity;
            }
            set
            {
                jobComplexity = value;
                OnPropertyChanged("JobComplexity");
            }
        }

        public DoubleRange Connectivity
        {
            get
            {
                return connectivity;
            }
            set
            {
                connectivity = value;
                OnPropertyChanged("Connectivity");
            }
        }
        
        public int TestRuns
        {
            get
            {
                return testRuns;
            }
            set
            {
                testRuns = value;
                OnPropertyChanged("TestRuns");
            }
        }

        #endregion

        public TestOptionsDialogViewModel()
        {
            Connectivity = new DoubleRange { From = 0.1, To = 0.9, Step = 0.1 };
            JobCount = new IntegerRange { From = 8, To = 32, Step = 8 };
            JobComplexity = new IntegerRange { From = 4, To = 16 };
            TestRuns = 2;
        }
    }
}
