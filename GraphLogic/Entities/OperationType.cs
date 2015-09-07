using System.Linq;
using System.Reflection;

using Entities;

namespace GraphLogic.Entities
{
    public sealed class OperationType
    {
        public static readonly OperationType Addition = new OperationType { Symbol = "+", FullName = "addition", DefaultComplexity = 1 };
        public static readonly OperationType Subtraction = new OperationType { Symbol = "-", FullName = "subtraction", DefaultComplexity = 1 };
        public static readonly OperationType Multiplication = new OperationType { Symbol = "*", FullName = "multiplication", DefaultComplexity = 2 };
        public static readonly OperationType Division = new OperationType { Symbol = "/", FullName = "division", DefaultComplexity = 4 };
        public static readonly OperationType Function = new OperationType { Symbol = "f()", FullName = "function", DefaultComplexity = 1 };

        private static readonly OperationType[] values = typeof(OperationType).
                                                 GetFields(BindingFlags.Public | BindingFlags.Static).
                                                 Where(field => field.IsInitOnly && field.FieldType == typeof(OperationType)).
                                                 Select(field => field.GetValue(null)).
                                                 Cast<OperationType>().
                                                 ToArray();

        public static OperationType[] Values
        {
            get { return values; }
        }

        public static OperationType FromFullName(string fullName)
        {
            return values.FirstOrDefault(op => op.FullName == fullName.ToLower());
        }

        public string Symbol { get; private set; }
        public string FullName { get; private set; }
        public int DefaultComplexity { get; private set; }

        public override string ToString()
        {
            return Symbol;
        }

        private OperationType() { }
    }
}
