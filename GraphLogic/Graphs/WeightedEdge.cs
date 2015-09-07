using Entities;
using GraphLogic.Entities;

using QuickGraph;

using System.Globalization;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace GraphLogic.Graphs
{
    public class WeightedEdge : EditableValueBase, IWeightedEdge<EditableValueBase>, IDeepClonable<WeightedEdge>
    {
        private double weight;

        private EditableValueBase source;
        private EditableValueBase target;

        [XmlAttribute("Weight")]
        public double Weight
        {
            get { return weight; }
            set
            {
                weight = value;
                RaisePropertyChanged("Weight");
            }
        }

        public WeightedEdge(EditableValueBase source, EditableValueBase target, double weight = 1) : base(0)
        {
            this.source = source;
            this.target = target;

            Weight = weight;
        }

        #region Overriden Object methods

        public override int GetHashCode()
        {
            return Source.GetHashCode() ^ (int)(Weight * 100) ^ Target.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            var o = obj as WeightedEdge;
            if (o == null)
                return false;

            if (object.ReferenceEquals(this, o))
                return true;

            return (Source == o.Source) && (Target == o.Target) && (Weight == o.Weight);
        }

        #endregion

        #region IEdge implementation

        public EditableValueBase Source
        {
            get { return source; }
        }

        public EditableValueBase Target
        {
            get { return target; }
        }

        #endregion

        #region IDeepClonable implementation

        public new WeightedEdge DeepCopy()
        {
            return new WeightedEdge(Source, Target, Weight);
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
            Weight = double.Parse(reader.GetAttribute("Weight"));
        }

        public override void WriteXml(XmlWriter writer)
        {
            writer.WriteAttributeString("Weight", Weight.ToString(CultureInfo.InvariantCulture));
        }

        #endregion
    }
}
