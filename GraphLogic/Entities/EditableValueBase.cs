using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

using Entities;

namespace GraphLogic.Entities
{
    public abstract class EditableValueBase : BindableBase, IDeepClonable<EditableValueBase>, IXmlSerializable
    {
        public int ID { get; protected set; }

        public EditableValueBase(int id)
        {
            ID = id;
        }

        IDeepClonable IDeepClonable.DeepCopy()
        {
            return DeepCopy();
        }

        public virtual EditableValueBase DeepCopy()
        {
            return MemberwiseClone() as EditableValueBase;
        }
    
        public abstract XmlSchema GetSchema();

        public abstract void ReadXml(XmlReader reader);

        public abstract void WriteXml(XmlWriter writer);
    }
}
