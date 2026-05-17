using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TaskManagement.Datas;
using TaskManagement.Interfaces;
using TaskManagement.Models;

namespace TaskManagement.Services
{
    public class AdminService : IAdminService
    {
        private readonly TaskManagementContext _context;

        public AdminService(TaskManagementContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<User>> GetAllUsersAsync() => 
            await _context.Users.Include(u => u.Role).ToListAsync();

        public async Task<bool> DeleteUserAsync(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return false;
            _context.Users.Remove(user);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<IEnumerable<Taskstatustype>> GetAllStatusTypesAsync() => 
            await _context.Taskstatustypes.ToListAsync();

        public async Task<bool> CreateStatusTypeAsync(Taskstatustype statusType)
        {
            _context.Taskstatustypes.Add(statusType);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeleteStatusTypeAsync(int id)
        {
            var status = await _context.Taskstatustypes.FindAsync(id);
            if (status == null) return false;
            _context.Taskstatustypes.Remove(status);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<IEnumerable<Roletype>> GetAllRoleTypesAsync() => 
            await _context.Roletypes.ToListAsync();

        public async Task<bool> CreateRoleTypeAsync(Roletype roleType)
        {
            _context.Roletypes.Add(roleType);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeleteRoleTypeAsync(int id)
        {
            var role = await _context.Roletypes.FindAsync(id);
            if (role == null) return false;
            _context.Roletypes.Remove(role);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> UpdateUserRoleAsync(int userId, int roleTypeId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return false;

            // Kiểm tra xem Role Type mới có tồn tại hợp lệ không
            var roleExists = await _context.Roletypes.AnyAsync(r => r.Roletypeid == roleTypeId);
            if (!roleExists) return false;

            user.Roleid = roleTypeId;
            _context.Users.Update(user);
            return await _context.SaveChangesAsync() > 0;
        }
    }
}