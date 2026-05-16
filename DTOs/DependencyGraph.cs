namespace TaskManagement.DTOs
{
    public class DependencyGraph
    {
        public Dictionary<int, List<int>> AdjacencyList { get; set; } = new();
        public Dictionary<int, int> InDegree { get; set; } = new();
    }
}