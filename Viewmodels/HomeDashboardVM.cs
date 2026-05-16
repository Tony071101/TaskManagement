namespace TaskManagement.Viewmodels
{
    public class HomeDashboardVM
    {
        public int MyTasksCount { get; set; }
        public int PendingApprovalsCount { get; set; }
        public List<TaskManagement.Models.Task> RecentTasks { get; set; } = new List<TaskManagement.Models.Task>();
        public bool IsAuthenticated { get; set; }
    }
}