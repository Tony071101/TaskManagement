using Microsoft.AspNetCore.Mvc.Rendering;

namespace TaskManagement.Interfaces
{
    public interface ITaskService
    {
        Task<List<Models.Task>> GetDashboardTasksAsync(int userId, bool isAdmin, bool isUser, bool isChecker);
        Task<bool> IsTaskLockedAsync(int taskId);
        Task<bool> CanUserEditTaskAsync(int taskId, int userId, bool isAdmin);
        Task<bool> CompleteTaskAsync(int taskId, int checkeruserId);
        Task<bool> DeleteTaskAsync(int taskId);
        Task<(List<Models.Task> Tasks, int FilteredCount, int TotalCount)> GetFilteredTasksAsync(
            int start, int length, string[] columnFilters);
        Task<bool> CreateTaskWithFileAsync(Models.Task task, Microsoft.AspNetCore.Http.IFormFile? file);
        Task<IEnumerable<SelectListItem>> GetCategorySelectListAsync();
        Task<IEnumerable<SelectListItem>> GetUserSelectListAsync();
        Task<object> GetPagedTasksAsync(int start, int length, string[] columnFilters, int currentUserId, bool isAdmin);
        Task<Models.Task?> GetTaskByIdAsync(int id);
        Task<bool> AddDependencyAsync(int predecessorId, int successorId);
        Task<string> GetGitHubPrStatusAsync(string prUrl);
        Task<IEnumerable<SelectListItem>> GetStatusSelectListAsync();
        Task<bool> UpdateTaskAsync(Models.Task task, Microsoft.AspNetCore.Http.IFormFile? file);
    }
}