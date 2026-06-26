using AspireCRM.DataLayer.Repositories;
using AspireCRM.Domain.Common;

namespace AspireCRM.ApiService.Endpoints;

public static class CategoryEndpoints
{
    public static void MapCategoryEndpoints(this WebApplication app)
    {
        var api = app.MapGroup("/api/categories");

        api.MapGet("/", async (IRepository<Category> repo) =>
            Results.Ok(await repo.GetAllAsync()));

        api.MapGet("/{id:long}", async (long id, IRepository<Category> repo) =>
        {
            var category = await repo.GetByIdAsync(id);
            return category is null ? Results.NotFound() : Results.Ok(category);
        });

        api.MapPost("/", async (Category category, IRepository<Category> repo) =>
        {
            var created = await repo.AddAsync(category);
            return Results.Created($"/api/categories/{created.Id}", created);
        });

        api.MapPut("/{id:long}", async (long id, Category category, IRepository<Category> repo) =>
        {
            if (id != category.Id) return Results.BadRequest();
            await repo.UpdateAsync(category);
            return Results.NoContent();
        });

        api.MapDelete("/{id:long}", async (long id, IRepository<Category> repo) =>
        {
            var category = await repo.GetByIdAsync(id);
            if (category is null) return Results.NotFound();
            await repo.DeleteAsync(category);
            return Results.NoContent();
        });
    }
}