namespace TaskManagement.DTOs
{
    public class TaskIndexDto
    {
        public int Taskid { get; set; }
        public string Taskname { get; set; } = null!;
        public string Datetimedue { get; set; } = null!;
        public string PerformerName { get; set; } = "Chưa giao";
        public string CheckerName { get; set; } = "Chưa giao";
        public string StatusName { get; set; } = "N/A";
        public int Statusid { get; set; }
        public string CategoryName { get; set; } = "Chưa phân loại";
    }
}