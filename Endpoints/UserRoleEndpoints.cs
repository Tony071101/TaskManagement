using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using TaskManagement.Datas;
using TaskManagement.Models;

namespace TaskManagement.Endpoints
{
    public static class UserRoleEndpoints
    {
        public static void MapUserRoleEndpoints(this IEndpointRouteBuilder routes)
        {
            var group = routes.MapGroup("/api/userrole").WithTags("RoleTypes");

            group.MapGet("/", GetAllRoles);
            group.MapGet("/{id:int}", GetRoleById);
            group.MapPost("/", CreateRole);
            group.MapPut("/{id:int}", UpdateRole);
            group.MapDelete("/{id:int}", DeleteRole);
        }

        // Các phương thức xử lý (Handler) tách biệt
        private static async Task<Ok<List<Roletype>>> GetAllRoles(TaskManagementContext db)
        {
            var roles = await db.Roletypes.ToListAsync();
            return TypedResults.Ok(roles);
        }

        private static async Task<Results<Ok<Roletype>, NotFound>> GetRoleById(int id, TaskManagementContext db)
        {
            return await db.Roletypes.FindAsync(id) is Roletype role 
                ? TypedResults.Ok(role) 
                : TypedResults.NotFound();
        }

        private static async Task<Created<Roletype>> CreateRole(Roletype role, TaskManagementContext db)
        {
            db.Roletypes.Add(role);
            await db.SaveChangesAsync();
            return TypedResults.Created($"/api/userrole/{role.Roletypeid}", role);
        }

        private static async Task<Results<NoContent, NotFound>> UpdateRole(int id, Roletype inputRole, TaskManagementContext db)
        {
            var role = await db.Roletypes.FindAsync(id);
            if (role is null) return TypedResults.NotFound();

            role.Rolename = inputRole.Rolename;
            await db.SaveChangesAsync();
            return TypedResults.NoContent();
        }

        private static async Task<Results<Ok<Roletype>, NotFound>> DeleteRole(int id, TaskManagementContext db)
        {
            if (await db.Roletypes.FindAsync(id) is Roletype role)
            {
                db.Roletypes.Remove(role);
                await db.SaveChangesAsync();
                return TypedResults.Ok(role);
            }
            return TypedResults.NotFound();
        }
    }
}
