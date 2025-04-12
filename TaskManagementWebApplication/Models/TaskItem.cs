namespace TaskManagement.Models {
    public class TaskItem {
        public int Id { get; set;}
        
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Status { get; set; } = "To-Do";
        public DateTime DueDate { get; set; } = DateTime.UtcNow;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public int? AssignedToId { get; set; }
        public User? AssignedTo { get; set; }
    }
}