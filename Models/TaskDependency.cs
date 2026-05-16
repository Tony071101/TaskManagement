namespace TaskManagement.Models
{
    public class TaskDependency
    {
        public int PredecessorTaskId { get; set; }
        public int SuccessorTaskId { get; set; }

        // Navigation properties
        public virtual Task? PredecessorTask { get; set; }
        public virtual Task? SuccessorTask { get; set; }
    }
}