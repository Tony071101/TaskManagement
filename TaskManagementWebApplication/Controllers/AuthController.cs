using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using TaskManagement.Data;
using TaskManagement.Models;
using System.Security.Cryptography;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.SignalR;
using TaskManagement.Hubs;

namespace TaskManagement.Controllers {
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase {
        private readonly AppDbContext _context;
        private readonly IConfiguration _config;
        private readonly IHubContext<NotificationHub> _hubContext;
        public AuthController(AppDbContext context, IConfiguration config, IHubContext<NotificationHub> hubContext) {
            _context = context;
            _config = config;
            _hubContext = hubContext;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request) {
            Console.WriteLine($"Received register request: Name={request.Name}, Email={request.Email}, Password={request.Password}");

            if (request == null) {
                Console.WriteLine("Request body is null.");
                return BadRequest("Dữ liệu đăng ký không hợp lệ.");
            }

            if (string.IsNullOrWhiteSpace(request.Name) || string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password)) {
                Console.WriteLine("One or more required fields are empty.");
                return BadRequest("Tên, Email và Mật khẩu không được để trống.");
            }

            if (_context.Users.Any(u => u.Email == request.Email)) {
                Console.WriteLine($"Email {request.Email} đã tồn tại trong hệ thống.");
                return BadRequest("Email đã được sử dụng.");
            }

            if (_context.Users.Any(u => u.Name == request.Name)) {
                Console.WriteLine($"Tên {request.Name} đã tồn tại trong hệ thống.");
                return BadRequest("Tên đã được sử dụng.");
            }

            Console.WriteLine("Hashing password...");
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(request.Password, workFactor: 11);

            var newUser = new User {
                Name = request.Name,
                Email = request.Email,
                PasswordHash = hashedPassword,
            };

            try {
                Console.WriteLine("Adding new user to the database...");
                _context.Users.Add(newUser);
                await _context.SaveChangesAsync();
                Console.WriteLine("User successfully added to database.");
            } catch (Exception ex) {
                Console.WriteLine($"Error saving user to database: {ex.Message}");
                return StatusCode(500, new { message = "Lỗi khi lưu người dùng", details = ex.Message });
            }

            Console.WriteLine("Fetching all users for real-time update...");
            var users = await _context.Users.ToListAsync();
            await _hubContext.Clients.All.SendAsync("ReceiveUserUpdate", users);

            Console.WriteLine("User registration successful.");
            return Ok(new { message = "Đăng ký thành công." });
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest loginRequest) {
            Console.WriteLine($"Login attempt: Email={loginRequest.Email}, Password={loginRequest.Password.Trim()}");
            var existingUser = _context.Users.FirstOrDefault(u => u.Email == loginRequest.Email);
            if (existingUser == null) return Unauthorized("Sai email.");
            Console.WriteLine($"PasswordHash in DB: {existingUser.PasswordHash}");
            bool isPasswordValid = BCrypt.Net.BCrypt.Verify(loginRequest.Password.Trim(), existingUser.PasswordHash);
            if (!isPasswordValid) return Unauthorized("Mật khẩu không đúng.");
            
            var accessToken = GenerateJwtToken(existingUser);
            var refreshToken = existingUser.RefreshToken;
            
            if (string.IsNullOrEmpty(existingUser.RefreshToken) || existingUser.RefreshTokenExpiry < DateTime.UtcNow) {
                refreshToken = GenerateRefreshToken();
                existingUser.RefreshToken = refreshToken;
                existingUser.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7);
                _context.SaveChanges();
            }

            return Ok(new {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                user = new {
                    Id = existingUser.Id,
                    Name = existingUser.Name,
                    Email = existingUser.Email,
                }
            });
        }

        private string GenerateJwtToken(User user) {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"] ?? "default_secret_key"));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var claims = new[] {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var token = new JwtSecurityToken(
                _config["Jwt:Issuer"],
                _config["Jwt:Audience"],
                claims,
                expires: DateTime.UtcNow.AddMinutes(30),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        [HttpPost("refresh-token")]
        public IActionResult RefreshToken([FromBody] RefreshTokenRequest request) {
            var user = _context.Users.FirstOrDefault(u => u.RefreshToken == request.RefreshToken);
            if (user == null || user.RefreshTokenExpiry < DateTime.UtcNow) {
                return Unauthorized("Refresh token không hợp lệ hoặc đã hết hạn.");
            }

            var newAccessToken = GenerateJwtToken(user);
            var newRefreshToken = GenerateRefreshToken();

            user.RefreshToken = newRefreshToken;
            user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7);
            _context.SaveChanges();

            return Ok(new { accessToken = newAccessToken, refreshToken = newRefreshToken });
        }

        private string GenerateRefreshToken() {
            var randomNumber = new byte[32];
            using (var rng = RandomNumberGenerator.Create()) {
                rng.GetBytes(randomNumber);
            }
            return Convert.ToBase64String(randomNumber);
        }
    }

    public class LoginRequest {
        [Required]
        public required string Email { get; set; }
        [Required]
        public required string Password { get; set; }
    }

    public class RegisterRequest {
        [Required]
        public required string Name { get; set; }

        [Required]
        public required string Email { get; set; }

        [Required]
        public required string Password { get; set; }
    }

    public class RefreshTokenRequest {
        public required string RefreshToken { get; set; }
    }
}