using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using TaskManagement.Datas;
using TaskManagement.Models;
using TaskManagement.Viewmodels;
using TaskManagement.Interfaces;
using Microsoft.AspNetCore.Authorization;

namespace TaskManagement.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly TaskManagementContext _context;
        private readonly ITaskService _taskService;
        private readonly IAdminService _adminService;

        public HomeController(ILogger<HomeController> logger, TaskManagementContext context, ITaskService taskService, IAdminService adminService)
        {
            _logger = logger;
            _context = context;
            _taskService = taskService;
            _adminService = adminService;
        }

        public async Task<IActionResult> Index()
        {
            var viewModel = new HomeDashboardVM();

            if (User.Identity?.IsAuthenticated == true)
            {
                int userId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");
                viewModel.IsAuthenticated = true;
                
                viewModel.RecentTasks = await _taskService.GetDashboardTasksAsync(
                    userId, 
                    User.IsInRole("Admin"), 
                    User.IsInRole("User"), 
                    User.IsInRole("Checker")
                );
                viewModel.MyTasksCount = viewModel.RecentTasks.Count;
            }

            return View(viewModel);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AdminDashboard()
        {
            var viewModel = new AdminDashboardVM
            {
                Users = await _adminService.GetAllUsersAsync(),
                StatusTypes = await _adminService.GetAllStatusTypesAsync(),
                RoleTypes = await _adminService.GetAllRoleTypesAsync()
            };
            return View(viewModel);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateStatus(string statusName)
        {
            if (!string.IsNullOrEmpty(statusName))
            {
                await _adminService.CreateStatusTypeAsync(new Taskstatustype { Statusname = statusName });
                TempData["Success"] = "Thêm trạng thái thành công!";
            }
            return RedirectToAction(nameof(AdminDashboard));
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUser(int id)
        {
            await _adminService.DeleteUserAsync(id);
            return RedirectToAction(nameof(AdminDashboard));
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateRoleType(string roleName)
        {
            if (!string.IsNullOrEmpty(roleName))
            {
                var result = await _adminService.CreateRoleTypeAsync(new Roletype { Rolename = roleName });
                if (result)
                    TempData["Success"] = "Thêm vai trò hệ thống mới thành công!";
                else
                    TempData["Error"] = "Có lỗi xảy ra khi thêm vai trò.";
            }
            return RedirectToAction(nameof(AdminDashboard));
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateUserRole(int userId, int roleTypeId)
        {
            var result = await _adminService.UpdateUserRoleAsync(userId, roleTypeId);
            if (result)
            {
                TempData["Success"] = "Cập nhật phân quyền thành viên thành công!";
            }
            else
            {
                TempData["Error"] = "Không thể cập nhật quyền, vui lòng kiểm tra dữ liệu.";
            }
            return RedirectToAction(nameof(AdminDashboard));
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
