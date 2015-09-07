using GraphLogic.Entities;

using System.Linq;
using System.Collections.Generic;

namespace TaskPlanning.JobAssignment
{
    public class PcsOptions
    {
        private Dictionary<Processor, int> physicalLinks;
        private int defaultLinkCount;

        public bool IoControllerPresent { get; set; }
        public bool DuplexTransfer { get; set; }

        public IReadOnlyDictionary<Processor, int> PhysicalLinks
        {
            get { return physicalLinks; }
        }

        public PcsOptions(IEnumerable<Processor> processors) : this(processors, true, true) { }

        public PcsOptions(IEnumerable<Processor> processors, bool ioController, bool duplexTransfer)
        {
            IoControllerPresent = ioController;
            DuplexTransfer = duplexTransfer;

            physicalLinks = new Dictionary<Processor, int>();
            foreach (var proc in processors)
            {
                physicalLinks[proc] = 1;
            }
        }

        public void SetPhysicalLinks(Processor processor, int count)
        {
            physicalLinks[processor] = count;
        }

        public void SetForAll(int count)
        {
            foreach (var proc in PhysicalLinks.Keys.ToArray())
            {
                physicalLinks[proc] = count;
            }
        }
    }
}
