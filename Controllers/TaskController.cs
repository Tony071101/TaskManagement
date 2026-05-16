using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskManagement.Datas;
using TaskManagement.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using AutoMapper;
using TaskManagement.Enums;
using TaskManagement.DTOs;
using System.Diagnostics;

namespace TaskManagement.Controllers
{
    [Authorize]
    public class TaskController : Controller
    {
        private readonly ITaskService _taskService;
        private readonly IMapper _mapper;

        public TaskController(IMapper mapper, ITaskService taskService) 
        { 
            _taskService = taskService; 
            _mapper = mapper;
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create()
        {
            await PopulateViewBagsForCreate();
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var task = await _taskService.GetTaskByIdAsync(id);
            if (task == null) return NotFound();

            var model = _mapper.Map<TaskUpdateDTO>(task);

            int currentUserId = GetUserId();
            bool isAdmin = User.IsInRole("Admin");

            if (!await _taskService.CanUserEditTaskAsync(id, currentUserId, isAdmin))
                return Forbid();

            if (!string.IsNullOrEmpty(model.GithubPrUrl))
            {
                model.GithubStatus = await _taskService.GetGitHubPrStatusAsync(model.GithubPrUrl);
            }

            ViewBag.CategoryList = await _taskService.GetCategorySelectListAsync();
            ViewBag.UserList = await _taskService.GetUserSelectListAsync();
            ViewBag.CheckerList = await _taskService.GetUserSelectListAsync();
            ViewBag.StatusList = await _taskService.GetStatusSelectListAsync();
            ViewBag.CurrentStatusName = task.Status?.Statusname;
            
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> GetTasksData()
        {
            int start = Convert.ToInt32(Request.Form["start"].FirstOrDefault() ?? "0");
            int length = Convert.ToInt32(Request.Form["length"].FirstOrDefault() ?? "10");
            string[] columnFilters = new string[6];
            for (int i = 0; i < 6; i++) 
                columnFilters[i] = Request.Form[$"columns[{i}][search][value]"].FirstOrDefault() ?? "";

            var result = await _taskService.GetPagedTasksAsync(start, length, columnFilters, GetUserId(), User.IsInRole("Admin"));
            
            return Json(new {
                draw = Request.Form["draw"].FirstOrDefault(),
                recordsTotal = ((dynamic)result).recordsTotal,
                recordsFiltered = ((dynamic)result).recordsFiltered,
                data = ((dynamic)result).data
            });
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateDependency(int predecessorId, int successorId)
        {
            var result = await _taskService.AddDependencyAsync(predecessorId, successorId);

            if (!result)
            {
                return BadRequest(new { message = "Không thể tạo mối quan hệ này vì sẽ gây ra vòng lặp hoặc dữ liệu không hợp lệ." });
            }

            return Ok(new { message = "Thêm phụ thuộc công việc thành công." });
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TaskCreateDTO model, IFormFile? attachment)
        {
            if (!ModelState.IsValid)
            {
                await PopulateViewBagsForCreate();
                return View(model);
            }

            var task = _mapper.Map<Models.Task>(model);

            task.Assigner = GetUserId();
            task.Statusid = (int)TaskStatusEnum.Pending;
            task.Datetimetaskcreated = DateTime.Now;
            var result = await _taskService.CreateTaskWithFileAsync(task, attachment);

            if (result)
            {
                return RedirectToAction("Index", "Home");
            }

            return View(model);
        }

        [HttpPost]
        [Authorize] // Cho phép tất cả người dùng đã đăng nhập vào
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, TaskUpdateDTO model, IFormFile? attachment)
        {
            if (id != model.Taskid) return BadRequest();

            // Lấy dữ liệu gốc từ DB để đảm bảo không bị ghi đè các trường khóa
            var existingTask = await _taskService.GetTaskByIdAsync(id);
            if (existingTask == null) return NotFound();

            if (User.IsInRole("Admin"))
            {
                // ADMIN: Ánh xạ toàn bộ dữ liệu từ Form vào Entity
                _mapper.Map(model, existingTask);
            }
            else
            {
                // ROLE KHÁC: 
                // 1. CHỈ cập nhật Statusid sang Done
                existingTask.Statusid = (int)TaskStatusEnum.Done;
                
                // 2. Nếu có cột thời gian hoàn thành trong Task.cs, hãy cập nhật nó
                // existingTask.Datetimecompleted = DateTime.Now; 

                // KHÔNG dùng _mapper.Map(model, existingTask) ở đây để bảo vệ các trường khác 
                // như Taskname, Assignedto, Datetimedue không bị thay đổi.
            }

            var result = await _taskService.UpdateTaskAsync(existingTask, attachment);

            if (result)
            {
                TempData["Success"] = "Cập nhật thành công!";
                return RedirectToAction("Index", "Home");
            }

            await PopulateViewBagsForEdit(id);
            return View(model);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _taskService.DeleteTaskAsync(id);

            if (result)
            {
                TempData["Success"] = "Xóa công việc thành công!";
            }
            else
            {
                TempData["Error"] = "Không tìm thấy công việc hoặc có lỗi xảy ra khi xóa.";
            }

            return RedirectToAction("Index", "Home"); // Chuyển hướng về trang chủ sau khi xóa 
        }

        private int GetUserId() => int.Parse(User.FindFirst("UserId")?.Value ?? "0");

        private async Task PopulateViewBagsForEdit(int id)
        {
            ViewBag.CategoryList = await _taskService.GetCategorySelectListAsync();
            ViewBag.UserList = await _taskService.GetUserSelectListAsync();
            ViewBag.CheckerList = await _taskService.GetUserSelectListAsync();
            ViewBag.StatusList = await _taskService.GetStatusSelectListAsync();
            
            var task = await _taskService.GetTaskByIdAsync(id);
            ViewBag.CurrentStatusName = task?.Status?.Statusname;
        }

        private async Task PopulateViewBagsForCreate()
        {
            ViewBag.CategoryList = await _taskService.GetCategorySelectListAsync();
            ViewBag.UserList = await _taskService.GetUserSelectListAsync();
            ViewBag.CheckerList = await _taskService.GetUserSelectListAsync();
        }
    }
}