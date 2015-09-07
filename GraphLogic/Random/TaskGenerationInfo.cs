using Entities;

namespace GraphLogic.Random
{
    public class TaskGenerationInfo : BindableBase 
    {
        private int operationCount;

        private int minComplexity;
        private int maxComplexity;
        
        private double connectivity;

        public int OperationCount
        {
            get
            {
                return operationCount;
            }
            set
            {
                operationCount = value;
                RaisePropertyChanged("OperationCount");
            }
        }

        public int MinComplexity
        {
            get
            {
                return minComplexity;
            }
            set
            {
                minComplexity = value;
                RaisePropertyChanged("MinComplexity");
            }
        }

        public int MaxComplexity
        {
            get
            {
                return maxComplexity;
            }
            set
            {
                maxComplexity = value;
                RaisePropertyChanged("MaxComplexity");
            }
        }

        public double Connectivity
        {
            get
            {
                return connectivity;
            }
            set
            {
                connectivity = value;
                RaisePropertyChanged("Connectivity");
            }
        }

        public TaskGenerationInfo()
        {
            OperationCount = 6;

            MinComplexity = 1;
            MaxComplexity = 7;

            Connectivity = 0.8;
        }
    }
}
