using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using TaskManagement.Viewmodels;
using TaskManagement.Interfaces;

namespace TaskManagement.Controllers
{
    public class AccountController : Controller
    {
        private readonly IAccountService _accountService;
        private readonly ILogger<AccountController> _logger;

        public AccountController(IAccountService accountService, ILogger<AccountController> logger)
        {
            _accountService = accountService;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Register() => View();

        [HttpPost]
        [ValidateAntiForgeryToken] 
        public async Task<IActionResult> Register(RegisterVM model)
        {
            if (!ModelState.IsValid) return View(model);

            var result = await _accountService.RegisterAsync(model);
            if (result)
            {
                _logger.LogInformation("Tài khoản '{Username}' đã đăng ký thành công.", model.Username);
                return RedirectToAction(nameof(Login));
            }

            ModelState.AddModelError(string.Empty, "Tên đăng nhập đã tồn tại hoặc hệ thống gặp sự cố.");
            return View(model);
        }

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginVM model, string? returnUrl = null)
        {
            if (!ModelState.IsValid) return View(model);

            var userDto = await _accountService.AuthenticateAsync(model.Username, model.Password);

            if (userDto != null)
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, userDto.Username),
                    new Claim(ClaimTypes.Role, userDto.RoleName),
                    new Claim("UserId", userDto.UserId.ToString())
                };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, 
                    new ClaimsPrincipal(claimsIdentity));

                _logger.LogInformation("Người dùng '{Username}' đăng nhập thành công.", userDto.Username);

                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    return Redirect(returnUrl);

                return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError(string.Empty, "Tên đăng nhập hoặc mật khẩu không chính xác.");
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            string? currentUsername = User.Identity?.Name;
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            
            _logger.LogInformation("Người dùng '{Username}' đã đăng xuất.", currentUsername ?? "Ẩn danh");
            return RedirectToAction(nameof(Login));
        }
    }
}