using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using TaskManagement.Datas;
using TaskManagement.Models;

namespace TaskManagement.Endpoints
{
    public static class TaskStatusEndpoints
    {
        public static void MapTaskStatusEndpoints(this IEndpointRouteBuilder routes)
        {
            var group = routes.MapGroup("/api/taskstatus").WithTags("TaskStatus");

            group.MapGet("/", GetAllTaskStatus);
            group.MapGet("/{id:int}", GetTaskStatusById);
            group.MapPost("/", CreateTaskStatus);
            group.MapPut("/{id:int}", UpdateTaskStatus);
            group.MapDelete("/{id:int}", DeleteTaskStatus);
        }

        // Các phương thức xử lý (Handler) tách biệt
        private static async Task<Ok<List<Taskstatustype>>> GetAllTaskStatus(TaskManagementContext db)
        {
            var taskstatus = await db.Taskstatustypes.ToListAsync();
            return TypedResults.Ok(taskstatus);
        }

        private static async Task<Results<Ok<Taskstatustype>, NotFound>> GetTaskStatusById(int id, TaskManagementContext db)
        {
            return await db.Taskstatustypes.FindAsync(id) is Taskstatustype taskstatus 
                ? TypedResults.Ok(taskstatus) 
                : TypedResults.NotFound();
        }

        private static async Task<Created<Taskstatustype>> CreateTaskStatus(Taskstatustype taskstatus, TaskManagementContext db)
        {
            db.Taskstatustypes.Add(taskstatus);
            await db.SaveChangesAsync();
            return TypedResults.Created($"/api/taskstatus/{taskstatus.Taskstatustypeid}", taskstatus);
        }

        private static async Task<Results<NoContent, NotFound>> UpdateTaskStatus(int id, Taskstatustype inputTasksStatus, TaskManagementContext db)
        {
            var taskstatus = await db.Taskstatustypes.FindAsync(id);
            if (taskstatus is null) return TypedResults.NotFound();

            taskstatus.Statusname = inputTasksStatus.Statusname;
            await db.SaveChangesAsync();
            return TypedResults.NoContent();
        }

        private static async Task<Results<Ok<Taskstatustype>, NotFound>> DeleteTaskStatus(int id, TaskManagementContext db)
        {
            if (await db.Taskstatustypes.FindAsync(id) is Taskstatustype taskStatus)
            {
                db.Taskstatustypes.Remove(taskStatus);
                await db.SaveChangesAsync();
                return TypedResults.Ok(taskStatus);
            }
            return TypedResults.NotFound();
        }
    }
}