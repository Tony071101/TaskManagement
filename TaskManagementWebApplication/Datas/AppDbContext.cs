using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using TaskManagement.Models;

namespace TaskManagement.Data {
    public class AppDbContext : DbContext {
        [UnconditionalSuppressMessage("Trimming", "IL2026", Justification = "EF Core không hỗ trợ trimming.")]
        [UnconditionalSuppressMessage("AOT", "IL3050", Justification = "EF Core không hỗ trợ AOT.")]
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) {}

        public DbSet<TaskItem> Tasks { get; set; }
        public DbSet<User> Users { get; set; }
    }
}