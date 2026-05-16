using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using TaskManagement.Datas;
using TaskManagement.Models;
using TaskManagement.Viewmodels;
using TaskManagement.Interfaces;

namespace TaskManagement.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly TaskManagementContext _context;
        private readonly ITaskService _taskService;

        public HomeController(ILogger<HomeController> logger, TaskManagementContext context, ITaskService taskService)
        {
            _logger = logger;
            _context = context;
            _taskService = taskService;
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

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
