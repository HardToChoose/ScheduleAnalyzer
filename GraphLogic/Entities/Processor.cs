using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

using Entities;

namespace GraphLogic.Entities
{
    public class Processor : EditableValueBase, IDeepClonable<Processor>
    {
        private int performance;

        [XmlAttribute("Performance")]
        public int Performance
        {
            get { return performance; }
            set
            {
                performance = value;
                RaisePropertyChanged("Performance");
            }
        }

        public Processor() : this(0) { }

        public Processor(int id, int performance = 1) : base(id)
        {
            Performance = performance;
        }

        #region Equals override

        public override int GetHashCode()
        {
            return ID;
        }

        public override bool Equals(object obj)
        {
            var o = obj as Processor;
            if (o == null)
                return false;

            if (object.ReferenceEquals(this, obj))
                return true;

            return (ID == o.ID) && (Performance == o.Performance);
        }

        #endregion

        #region IDeepClonable implementation

        public Processor DeepCopy()
        {
            return new Processor(ID, Performance);
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
            Performance = int.Parse(reader.GetAttribute("Performance"));
        }

        public override void WriteXml(XmlWriter writer)
        {
            writer.WriteAttributeString("ID", ID.ToString());
            writer.WriteAttributeString("Performance", Performance.ToString());
        }

        #endregion
    }
}
