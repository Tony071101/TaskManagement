using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using TaskManagement.Data;
using TaskManagement.Hubs;
using TaskManagement.Models;

namespace TaskManagement.Controllers {
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase {
        private readonly AppDbContext _context;
        private readonly IHubContext<NotificationHub> _hubContext;

        public UsersController(AppDbContext context, IHubContext<NotificationHub> hubContext) {
            _context = context;
            _hubContext = hubContext;
        }

        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] User updatedUser) {
            if (id != updatedUser.Id) return BadRequest("User ID can't be changed.");
            
            var user = await _context.Users.FindAsync(id);
            if(user == null) return NotFound();
            
            user.Name = updatedUser.Name;
            user.Email = updatedUser.Email;

            if (!string.IsNullOrEmpty(updatedUser.PasswordHash)) {
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(updatedUser.PasswordHash);
            }
            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            var users = await _context.Users.ToListAsync();
            await _hubContext.Clients.All.SendAsync("ReceiveUserUpdate", users);

            return NoContent();
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<User>>> GetUsers() {
            return await _context.Users.ToListAsync();
        }
    }
}
