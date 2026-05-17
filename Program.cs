using Microsoft.EntityFrameworkCore;
using TaskManagement.Datas;
using TaskManagement.Endpoints;
using Microsoft.AspNetCore.Authentication.Cookies;
using TaskManagement.Interfaces;
using TaskManagement.Services;
using Octokit;

var builder = WebApplication.CreateBuilder(args);

//Chuỗi connect DB
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

//Đăng ký DBContext
builder.Services.AddDbContext<TaskManagementContext>(options =>
    options.UseNpgsql(connectionString));

//Cookie Authentication config
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
    });

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddAutoMapper(typeof(Program).Assembly);
builder.Services.AddScoped<ITaskService, TaskService>();
builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddScoped<IAdminService, AdminService>();
builder.Services.AddScoped<IGitHubClient>(sp => 
    new GitHubClient(new ProductHeaderValue("TaskManagement")));

var app = builder.Build();

app.MapUserRoleEndpoints();
app.MapCategoryEndpoints();
app.MapTaskStatusEndpoints();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
