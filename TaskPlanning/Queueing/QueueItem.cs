using GraphLogic.Entities;

namespace TaskPlanning.Queueing
{
    public class QueueItem
    {
        public Operation Value { get; set; }
        public string[] Parameters { get; set; }
    }
}
