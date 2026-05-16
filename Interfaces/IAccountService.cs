using TaskManagement.DTOs;
using TaskManagement.Viewmodels;

namespace TaskManagement.Interfaces
{
    public interface IAccountService
    {
        Task<bool> RegisterAsync(RegisterVM model);
        Task<AuthenticatedUserDTO?> AuthenticateAsync(string username, string password);
        Task<bool> IsUsernameExistsAsync(string username);
    }
}