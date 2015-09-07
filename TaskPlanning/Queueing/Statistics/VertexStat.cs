namespace TaskPlanning.Queueing.Statistics
{
    public class VertexStat
    {
        public double RootCriticalVertexPathLength { get; set; }
        public double RootCriticalTimePathSum { get; set; }

        public double LeafCriticalVertexPathLength { get; set; }
        public double LeafCriticalTimePathSum { get; set; }
    }
}
