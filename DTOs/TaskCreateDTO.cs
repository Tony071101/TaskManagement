namespace TaskManagement.DTOs
{
    public class TaskCreateDTO
    {
        public required string Taskname { get; set; }
        public DateTime Datetimedue { get; set; }
        public int Categoryid { get; set; }
        public int? Assignedto { get; set; }
        public int? Taskchecker { get; set; }
        public string? Description { get; set; }
        public string? GithubPrUrl { get; set; }
    }
}