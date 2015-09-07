using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

using Entities;

namespace GraphLogic.Entities
{
    public class Operation : EditableValueBase, IDeepClonable<Operation>, IUniqueObject
    {
        private OperationType type;
        private int complexity;

        public OperationType Type
        {
            get { return type; }
            set
            {
                type = value;
                RaisePropertyChanged("Type");
            }
        }

        [XmlAttribute("Complexity")]
        public int Complexity
        {
            get { return complexity; }
            set
            {
                complexity = value;
                RaisePropertyChanged("Complexity");
            }
        }

        public Operation() : this(0) { }

        public Operation(int id, OperationType type = null) : base(id)
        {
            Type = type ?? OperationType.Function;
            Complexity = Type.DefaultComplexity;
        }

        #region Overriden methods

        public override int GetHashCode()
        {
            return ID;
        }

        public override bool Equals(object obj)
        {
            var o = obj as Operation;
            if (o == null)
                return false;

            if (object.ReferenceEquals(this, o))
                return true;

            return (ID == o.ID) && (Complexity == o.Complexity) && (Type == o.Type);
        }

        #endregion

        #region IDeepClonable implementation

        public Operation DeepCopy()
        {
            return new Operation(ID, Type)
            {
                Complexity = Complexity
            };
        }

        IDeepClonable IDeepClonable.DeepCopy()
        {
            return DeepCopy();
        }

        #endregion

        #region IXmlSerializable implementation

        public override XmlSchema GetSchema()
        {
            return null;
        }

        public override void ReadXml(XmlReader reader)
        {
            reader.MoveToContent();

            ID = int.Parse(reader.GetAttribute("ID"));
            Type = OperationType.FromFullName(reader.GetAttribute("Type"));
            Complexity = int.Parse(reader.GetAttribute("Complexity"));
        }

        public override void WriteXml(XmlWriter writer)
        {
            writer.WriteAttributeString("ID", ID.ToString());
            writer.WriteAttributeString("Type", Type.FullName);
            writer.WriteAttributeString("Complexity", Complexity.ToString());
        }

        #endregion
    }
}
