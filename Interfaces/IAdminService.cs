using TaskManagement.Models;

namespace TaskManagement.Interfaces
{
    public interface IAdminService
    {
        Task<IEnumerable<User>> GetAllUsersAsync();
        Task<bool> DeleteUserAsync(int id);

        Task<IEnumerable<Taskstatustype>> GetAllStatusTypesAsync();
        Task<bool> CreateStatusTypeAsync(Taskstatustype statusType);
        Task<bool> DeleteStatusTypeAsync(int id);

        Task<IEnumerable<Roletype>> GetAllRoleTypesAsync();
        Task<bool> CreateRoleTypeAsync(Roletype roleType);
        Task<bool> DeleteRoleTypeAsync(int id);
        Task<bool> UpdateUserRoleAsync(int userId, int roleTypeId);
    }
}