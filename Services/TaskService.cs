using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Octokit;
using TaskManagement.Datas;
using TaskManagement.Enums;
using TaskManagement.Interfaces;
using TaskManagement.Models;

namespace TaskManagement.Services
{
    public class TaskService : ITaskService
    {
        private readonly TaskManagementContext _context;
        private readonly IGitHubClient _githubClient;

        public TaskService(TaskManagementContext context, IGitHubClient githubClient)
        {
            _context = context; 
            _githubClient = githubClient;
        } 

        public async Task<bool> CanUserEditTaskAsync(int taskId, int userId, bool isAdmin)
        {
            if (isAdmin) return true;

            var task = await _context.Tasks
                .AsNoTracking() 
                .FirstOrDefaultAsync(t => t.Taskid == taskId);
            if (task == null) return false;

            bool isRelated = task.Assignedto == userId || task.Taskchecker == userId;
            bool isLocked = await IsTaskLockedAsync(taskId);

            return isRelated && !isLocked;
        }

        public async Task<bool> CompleteTaskAsync(int taskId, int currentUserId)
        {
            var task = await _context.Tasks.Include(t => t.AssignedtoNavigation).FirstOrDefaultAsync(t => t.Taskid == taskId);

            if (task == null || task.Taskchecker != currentUserId) return false;

            using var transaction = await _context.Database.BeginTransactionAsync(); //Tìm hiểu về BeginTransactionAsync
            try
            {
                task.Statusid = (int)TaskStatusEnum.Done;

                if (task.AssignedtoNavigation != null)
                {
                    task.AssignedtoNavigation.Totaltaskcompleted = (task.AssignedtoNavigation.Totaltaskcompleted ?? 0) + 1;
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                return false;
            }
        }

        public async Task<List<Models.Task>> GetDashboardTasksAsync(int userId, bool isAdmin, bool isUser, bool isChecker)
        {
            var query = _context.Tasks
                .Include(t => t.Status)
                .Include(t => t.AssignedtoNavigation)
                .Include(t => t.TaskcheckerNavigation)
                .Include(t => t.Category)
                .AsQueryable();

            if (!isAdmin)
            {
                query = query.Where(t => 
                    (isUser && t.Assignedto == userId) || 
                    (isChecker && t.Taskchecker == userId)
                );
            }

            return await query.OrderByDescending(t => t.Datetimetaskcreated)
                            .Take(5)
                            .ToListAsync();
        }

        public async Task<bool> IsTaskLockedAsync(int taskId)
        {
            var task = await _context.Tasks
                .Include(t => t.Status)
                .FirstOrDefaultAsync(t => t.Taskid == taskId);
                
            if (task == null || task.Status == null) return false;

            return task.Status.Statusname == "Released" || task.Status.Statusname == "Rejected";
        }

        public async Task<bool> DeleteTaskAsync(int taskId)
        {
            var task = await _context.Tasks.FindAsync(taskId);
            if (task == null) return false;

            //Hiện tại đang theo 1 logic là không cho xóa task khi ở Released để dễ dàng Audit, 
            //có thể thay thể
            //bằng biện pháp xóa mềm 
            //(thêm bool vào, khi muốn thì cho bool = true để ẩn task hoàn thành đi)
            if (task.Statusid == (int)TaskStatusEnum.Released) return false;

            _context.Tasks.Remove(task);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<(List<Models.Task> Tasks, int FilteredCount, int TotalCount)> GetFilteredTasksAsync(
            int start, int length, string[] columnFilters)
        {
            var query = _context.Tasks
                .Include(t => t.Status)
                .Include(t => t.AssignedtoNavigation)
                .Include(t => t.TaskcheckerNavigation)
                .Include(t => t.Category)
                .AsQueryable();

            int totalCount = await query.CountAsync();

            if (!string.IsNullOrEmpty(columnFilters[0]))
                query = query.Where(t => t.Taskname.ToLower().Contains(columnFilters[0].ToLower()));

            if (!string.IsNullOrEmpty(columnFilters[1]))
                if (DateTime.TryParse(columnFilters[1], out var date))
                    query = query.Where(t => t.Datetimedue.Date == date.Date);

            if (!string.IsNullOrEmpty(columnFilters[2]))
                query = query.Where(t => t.AssignedtoNavigation != null && t.AssignedtoNavigation.Username.ToLower().Contains(columnFilters[2].ToLower()));

            if (!string.IsNullOrEmpty(columnFilters[3]))
                query = query.Where(t => t.TaskcheckerNavigation != null && t.TaskcheckerNavigation.Username.ToLower().Contains(columnFilters[3].ToLower()));

            if (!string.IsNullOrEmpty(columnFilters[4]))
                query = query.Where(t => t.Status != null && t.Status.Statusname.ToLower().Contains(columnFilters[4].ToLower()));

            if (!string.IsNullOrEmpty(columnFilters[5]))
                query = query.Where(t => t.Category != null && t.Category.Categoryname.ToLower().Contains(columnFilters[5].ToLower()));

            int filteredCount = await query.CountAsync();

            var data = await query.OrderByDescending(t => t.Datetimetaskcreated)
                                .Skip(start)
                                .Take(length)
                                .ToListAsync();

            return (data, filteredCount, totalCount);
        }

        public async Task<bool> CreateTaskWithFileAsync(Models.Task task, IFormFile? file)
        {
            if (file != null && file.Length > 0)
            {
                // Tạo đường dẫn lưu file trong wwwroot/uploads
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads", fileName);

                using (var stream = new FileStream(filePath, System.IO.FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }
                task.attachmenturl = "/uploads/" + fileName;
            }

            _context.Tasks.Add(task);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<IEnumerable<SelectListItem>> GetCategorySelectListAsync()
        {
            return await _context.Taskcategories
                .Select(c => new SelectListItem { Value = c.Categoryid.ToString(), Text = c.Categoryname })
                .ToListAsync();
        }

        public async Task<IEnumerable<SelectListItem>> GetUserSelectListAsync()
        {
            return await _context.Users
                .Select(u => new SelectListItem { Value = u.Userid.ToString(), Text = u.Username })
                .ToListAsync();
        }

        public async Task<object> GetPagedTasksAsync(int start, int length, string[] columnFilters, int currentUserId, bool isAdmin)
        {
            var (tasks, filteredCount, totalCount) = await GetFilteredTasksAsync(start, length, columnFilters);

            var result = tasks.Select(t => {
                bool isLocked = t.Status?.Statusname == "Released" || t.Status?.Statusname == "Rejected";
                return new {
                    t.Taskid,
                    t.Taskname,
                    Datetimedue = t.Datetimedue.ToString("dd/MM/yyyy"),
                    PerformerName = t.AssignedtoNavigation?.Username ?? "Chưa giao",
                    CheckerName = t.TaskcheckerNavigation?.Username ?? "Chưa giao",
                    StatusName = t.Status?.Statusname ?? "N/A",
                    t.Statusid,
                    CategoryName = t.Category?.Categoryname ?? "Chưa phân loại",
                    CanEdit = isAdmin || ((t.Assignedto == currentUserId || t.Taskchecker == currentUserId) && !isLocked)
                };
            });

            return new { data = result, recordsFiltered = filteredCount, recordsTotal = totalCount };
        }

        public async Task<Models.Task?> GetTaskByIdAsync(int id)
        {
            return await _context.Tasks
                .Include(t => t.Status)                
                .Include(t => t.Category)              
                .Include(t => t.AssignedtoNavigation)   
                .Include(t => t.TaskcheckerNavigation)  
                .FirstOrDefaultAsync(t => t.Taskid == id);
        }

        public async Task<bool> IsCycleAfterAddingDependency(int predecessorId, int successorId)
        {
            // 1. Lấy tất cả các mối quan hệ hiện có từ DB
            var dependencies = await _context.TaskDependencies.ToListAsync();
            
            // 2. Xây dựng đồ thị (giả định đã thêm mối quan hệ mới)
            var allTaskIds = await _context.Tasks.Select(t => t.Taskid).ToListAsync();
            var graph = new Dictionary<int, List<int>>();
            var inDegree = new Dictionary<int, int>();

            foreach (var id in allTaskIds) {
                graph[id] = new List<int>();
                inDegree[id] = 0;
            }

            // Nạp các cạnh hiện tại
            foreach (var dep in dependencies) {
                graph[dep.PredecessorTaskId].Add(dep.SuccessorTaskId);
                inDegree[dep.SuccessorTaskId]++;
            }

            // Nạp cạnh mới đang muốn thêm vào
            graph[predecessorId].Add(successorId);
            inDegree[successorId]++;

            // 3. Thuật toán Kahn
            var queue = new Queue<int>();
            foreach (var id in allTaskIds) {
                if (inDegree[id] == 0) queue.Enqueue(id);
            }

            int visitedCount = 0;
            while (queue.Count > 0)
            {
                int u = queue.Dequeue();
                visitedCount++;

                foreach (var v in graph[u])
                {
                    inDegree[v]--;
                    if (inDegree[v] == 0) queue.Enqueue(v);
                }
            }

            // Nếu số lượng node thăm được < tổng số node -> Có vòng lặp!
            return visitedCount < allTaskIds.Count;
        }

        public async Task<bool> AddDependencyAsync(int predecessorId, int successorId)
        {
            if (predecessorId == successorId) return false;

            // 1. Lấy toàn bộ danh sách Task và các mối quan hệ hiện có
            var allTaskIds = await _context.Tasks.Select(t => t.Taskid).ToListAsync();
            var currentDeps = await _context.TaskDependencies.ToListAsync();

            // 2. Xây dựng đồ thị (Adjacency List) và mảng In-Degree
            var adj = allTaskIds.ToDictionary(id => id, id => new List<int>());
            var inDegree = allTaskIds.ToDictionary(id => id, id => 0);

            foreach (var dep in currentDeps)
            {
                adj[dep.PredecessorTaskId].Add(dep.SuccessorTaskId);
                inDegree[dep.SuccessorTaskId]++;
            }

            // 3. Giả định thêm cạnh mới vào đồ thị để kiểm tra
            if (!adj[predecessorId].Contains(successorId))
            {
                adj[predecessorId].Add(successorId);
                inDegree[successorId]++;
            }

            // 4. Thuật toán Kahn để kiểm tra chu trình (Cycle Detection)
            var queue = new Queue<int>();
            foreach (var kvp in inDegree)
            {
                if (kvp.Value == 0) queue.Enqueue(kvp.Key);
            }

            int count = 0;
            while (queue.Count > 0)
            {
                int u = queue.Dequeue();
                count++;

                foreach (var v in adj[u])
                {
                    inDegree[v]--;
                    if (inDegree[v] == 0) queue.Enqueue(v);
                }
            }

            // Nếu count != tổng số node thì có vòng lặp
            bool hasCycle = count != allTaskIds.Count;

            if (hasCycle) return false;

            // 5. Nếu không có vòng lặp, tiến hành lưu vào DB
            var exists = await _context.TaskDependencies
                .AnyAsync(td => td.PredecessorTaskId == predecessorId && td.SuccessorTaskId == successorId);
            
            if (!exists)
            {
                _context.TaskDependencies.Add(new TaskDependency 
                { 
                    PredecessorTaskId = predecessorId, 
                    SuccessorTaskId = successorId 
                });
                return await _context.SaveChangesAsync() > 0;
            }

            return true; 
        }

        public async Task<string> GetGitHubPrStatusAsync(string prUrl)
        {
            if (string.IsNullOrWhiteSpace(prUrl)) return "No Link";

            try {
                var parts = prUrl.Replace("https://github.com/", "").Split('/');
                string owner = parts[0];
                string repo = parts[1];
                int prNumber = int.Parse(parts[3]);

                var pr = await _githubClient.PullRequest.Get(owner, repo, prNumber);
                
                if (pr.Merged) return "Merged";
                return pr.State.Value.ToString();
            }
            catch {
                return "Invalid/Private";
            }
        }

        public async Task<IEnumerable<SelectListItem>> GetStatusSelectListAsync()
        {
            return await _context.Taskstatustypes 
                .Select(s => new SelectListItem 
                { 
                    Value = s.Taskstatustypeid.ToString(), 
                    Text = s.Statusname 
                })
                .ToListAsync();
        }

        public async Task<bool> UpdateTaskAsync(Models.Task task, IFormFile? file)
        {
            if (file != null && file.Length > 0)
            {
                // Xử lý lưu file tương tự như hàm Create
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads", fileName);

                // Lưu file mới
                using (var stream = new FileStream(filePath, System.IO.FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                // Cập nhật đường dẫn file mới vào Task
                task.attachmenturl = "/uploads/" + fileName;
            }

            _context.Tasks.Update(task);
            return await _context.SaveChangesAsync() > 0;
        }
    }
}