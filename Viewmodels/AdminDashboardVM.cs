using TaskManagement.Models;

namespace TaskManagement.Viewmodels
{
    public class AdminDashboardVM
    {
        public IEnumerable<User> Users { get; set; } = new List<User>();
        public IEnumerable<Taskstatustype> StatusTypes { get; set; } = new List<Taskstatustype>();
        public IEnumerable<Roletype> RoleTypes { get; set; } = new List<Roletype>();
    }
}