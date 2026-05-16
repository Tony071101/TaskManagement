using AutoMapper;
using Microsoft.EntityFrameworkCore;
using TaskManagement.Datas;
using TaskManagement.DTOs;
using TaskManagement.Interfaces;
using TaskManagement.Models;
using TaskManagement.Viewmodels;

namespace TaskManagement.Services
{
    public class AccountService : IAccountService
    {
        private readonly TaskManagementContext _context;
        private readonly ILogger<AccountService> _logger;
        private readonly IMapper _mapper;

        public AccountService(TaskManagementContext context, ILogger<AccountService> logger, IMapper mapper)
        {
            _context = context;
            _logger = logger;
            _mapper = mapper;
        }

        public async Task<bool> IsUsernameExistsAsync(string username)
        {
            return await _context.Users.AnyAsync(u => u.Username == username);
        }

        public async Task<bool> RegisterAsync(RegisterVM model)
        {
            try
            {
                if (await IsUsernameExistsAsync(model.Username)) 
                {
                    _logger.LogWarning("Đăng ký thất bại: Tài khoản '{Username}' đã tồn tại.", model.Username);
                    return false;
                }

                var newUser = new User
                {
                    Username = model.Username,
                    Email = model.Email,
                    Password = BCrypt.Net.BCrypt.HashPassword(model.Password),
                    Roleid = 2, // Mặc định: User (Sau này sẽ chuyển thành hằng số / Enum)
                    Totaltaskcompleted = 0
                };

                _context.Users.Add(newUser);
                return await _context.SaveChangesAsync() > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi hệ thống khi đăng ký tài khoản cho '{Username}'.", model.Username);
                return false;
            }
        }

        public async Task<AuthenticatedUserDTO?> AuthenticateAsync(string username, string password)
        {
            var user = await _context.Users.Include(u => u.Role).FirstOrDefaultAsync(u => u.Username == username);
    
            if (user != null && BCrypt.Net.BCrypt.Verify(password, user.Password))
            {
                return _mapper.Map<AuthenticatedUserDTO>(user);
            }
            return null;
        }
    }
}