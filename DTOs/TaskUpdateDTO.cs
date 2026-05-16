namespace TaskManagement.DTOs
{
    public class TaskUpdateDTO
    {
        public int Taskid { get; set; }
        public required string Taskname { get; set; }

        public int Categoryid { get; set; }
        public int Statusid { get; set; }
        public int? Assignedto { get; set; }
        public int? Taskchecker { get; set; }

        public string? CategoryName { get; set; }
        public string? AssignedtoName { get; set; }
        public string? CheckerName { get; set; }
        public string? Description { get; set; }
        public string? AttachmentUrl { get; set; }
        public string? GithubPrUrl { get; set; }
        public string? GithubStatus { get; set; }
    }
}