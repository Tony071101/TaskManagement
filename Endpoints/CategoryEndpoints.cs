using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using TaskManagement.Datas;
using TaskManagement.Models;

namespace TaskManagement.Endpoints
{
    public static class CategoryEndpoints
    {
        public static void MapCategoryEndpoints(this IEndpointRouteBuilder routes)
        {
            var group = routes.MapGroup("/api/category").WithTags("Categories");

            group.MapGet("/", GetAllCategories);
            group.MapGet("/{id:int}", GetCategoryById);
            group.MapPost("/", CreateCategory);
            group.MapPut("/{id:int}", UpdateCategory);
            group.MapDelete("/{id:int}", DeleteCategory);
        }

        // Các phương thức xử lý (Handler) tách biệt
        private static async Task<Ok<List<Taskcategory>>> GetAllCategories(TaskManagementContext db)
        {
            var categories = await db.Taskcategories.ToListAsync();
            return TypedResults.Ok(categories);
        }

        private static async Task<Results<Ok<Taskcategory>, NotFound>> GetCategoryById(int id, TaskManagementContext db)
        {
            return await db.Taskcategories.FindAsync(id) is Taskcategory category 
                ? TypedResults.Ok(category) 
                : TypedResults.NotFound();
        }

        private static async Task<Created<Taskcategory>> CreateCategory(Taskcategory category, TaskManagementContext db)
        {
            db.Taskcategories.Add(category);
            await db.SaveChangesAsync();
            return TypedResults.Created($"/api/category/{category.Categoryid}", category);
        }

        private static async Task<Results<NoContent, NotFound>> UpdateCategory(int id, Taskcategory inputCategory, TaskManagementContext db)
        {
            var category = await db.Taskcategories.FindAsync(id);
            if (category is null) return TypedResults.NotFound();

            category.Categoryname = inputCategory.Categoryname;
            await db.SaveChangesAsync();
            return TypedResults.NoContent();
        }

        private static async Task<Results<Ok<Taskcategory>, NotFound>> DeleteCategory(int id, TaskManagementContext db)
        {
            if (await db.Taskcategories.FindAsync(id) is Taskcategory category)
            {
                db.Taskcategories.Remove(category);
                await db.SaveChangesAsync();
                return TypedResults.Ok(category);
            }
            return TypedResults.NotFound();
        }
    }
}