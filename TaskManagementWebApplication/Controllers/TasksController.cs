using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskManagement.Data;
using TaskManagement.Models;
using TaskManagement.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace TaskManagement.Controllers {
    [Route("api/[controller]")]
    [ApiController]
    public class TasksController : ControllerBase {
        private readonly AppDbContext _context;
        private readonly IHubContext<NotificationHub> _hubContext;
        public TasksController(AppDbContext context, IHubContext<NotificationHub> hubContext) {
            _context = context;
            _hubContext = hubContext;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<TaskItem>>> GetTask() {
            var tasks = await _context.Tasks
                .Include(t => t.AssignedTo)
                .ToListAsync();

            var now = DateTime.UtcNow;
            foreach (var task in tasks) {
                if (task.DueDate < now && task.Status != "Done") {
                    task.Status = "Overdue";
                }
            }
            await _context.SaveChangesAsync();

            return tasks;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<TaskItem>> GetTask(int id) {
            var task = await _context.Tasks.FindAsync(id);
            if(task ==  null) return NotFound();
            return task;
        }

        [HttpPost]
        public async Task<ActionResult<TaskItem>> CreateTask(TaskItem task) {
            if (string.IsNullOrWhiteSpace(task.Title)) return BadRequest("Title is required.");
            
            if(task.AssignedToId.HasValue) {
                var user = await _context.Users.FindAsync(task.AssignedToId.Value);
                if (user == null) return BadRequest("User does not exist.");
            }

            var newTask = new TaskItem{
                Title = task.Title,
                Description = task.Description,
                Status = "To-Do",
                DueDate = task.DueDate,
                AssignedToId = task.AssignedToId,
                CreatedAt = DateTime.UtcNow
            };

            _context.Tasks.Add(newTask);
            await _context.SaveChangesAsync();

            var tasks = await _context.Tasks.ToListAsync();
            await _hubContext.Clients.All.SendAsync("ReceiveTaskUpdate", tasks);
            
            return CreatedAtAction(nameof(GetTask), new { id = newTask.Id }, newTask);
        }
        
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTask(int id, TaskItem updatedTask)
        {
            if (id != updatedTask.Id) return BadRequest();

            var existingTask = await _context.Tasks.FindAsync(id);
            if (existingTask == null) return NotFound();

            if (existingTask.AssignedToId != updatedTask.AssignedToId)
            {
                return Unauthorized("You do not have permission to update the task.");
            }

            if (updatedTask.Status != "In Progress" && updatedTask.Status != "Done")
            {
                return BadRequest("Invalid status.");
            }

            existingTask.Title = updatedTask.Title;
            existingTask.Description = updatedTask.Description;
            existingTask.Status = updatedTask.Status;
            existingTask.DueDate = updatedTask.DueDate;
            existingTask.AssignedToId = updatedTask.AssignedToId;

            if (existingTask.DueDate < DateTime.UtcNow && existingTask.Status != "Done")
            {
                existingTask.Status = "Overdue";
            }

            await _context.SaveChangesAsync();

            var tasks = await _context.Tasks.ToListAsync();
            await _hubContext.Clients.All.SendAsync("ReceiveTaskUpdate", tasks);

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<TaskItem>> DeleteTask(int id) {
            var task = await _context.Tasks.FindAsync(id);
            if (task == null) return NotFound();

            _context.Tasks.Remove(task);
            await _context.SaveChangesAsync();

            var tasks = await _context.Tasks.ToListAsync();
            await _hubContext.Clients.All.SendAsync("ReceiveTaskUpdate", tasks);

            return NoContent();
        }
    }    
}