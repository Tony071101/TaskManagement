namespace TaskManagement.DTOs
{
    public class TaskDeleteDTO
    {
        public int Taskid { get; set; }
        public required string Taskname { get; set; }
        public required string StatusName { get; set; }
        public DateTime Datetaskcreated { get; set; }
    }
}