using AspireCRM.Domain.Tasks;
using AspireCRM.DataLayer;
using AspireCRM.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace AspireCRM.ApiService.Endpoints;

public static class TaskEndpoints
{
    public static void MapTaskEndpoints(this WebApplication app)
    {
        var api = app.MapGroup("/api/tasks");

        api.MapGet("/by-entity/{entityType}/{entityId:long}", async (
            string entityType, long entityId, AspireCRMDbContext db) =>
        {
            var tasks = await db.Set<CrmTask>()
                .Where(t => t.EntityType == entityType && t.EntityId == entityId && !t.IsDeleted)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();
            return Results.Ok(tasks);
        });

        api.MapGet("/{id:long}", async (long id, AspireCRMDbContext db) =>
        {
            var task = await db.Set<CrmTask>().FindAsync(id);
            return task is null || task.IsDeleted ? Results.NotFound() : Results.Ok(task);
        });

        api.MapPost("/", async (CrmTask task, AspireCRMDbContext db, ITenantService tenantService) =>
        {
            task.CreatedAt = DateTime.UtcNow;
            if (tenantService.TenantId.HasValue)
                task.TenantId = tenantService.TenantId.Value;
            db.Set<CrmTask>().Add(task);
            await db.SaveChangesAsync();
            return Results.Created($"/api/tasks/{task.Id}", task);
        });

        api.MapPut("/{id:long}", async (long id, CrmTask updated, AspireCRMDbContext db) =>
        {
            var task = await db.Set<CrmTask>().FindAsync(id);
            if (task is null || task.IsDeleted)
                return Results.NotFound();

            task.Title = updated.Title;
            task.Description = updated.Description;
            task.DueDate = updated.DueDate;
            task.IsCompleted = updated.IsCompleted;
            task.AssignedToId = updated.AssignedToId;
            if (updated.IsCompleted && !task.CompletedAt.HasValue)
                task.CompletedAt = DateTime.UtcNow;
            else if (!updated.IsCompleted)
                task.CompletedAt = null;

            await db.SaveChangesAsync();
            return Results.Ok(task);
        });

        api.MapDelete("/{id:long}", async (long id, AspireCRMDbContext db) =>
        {
            var task = await db.Set<CrmTask>().FindAsync(id);
            if (task is null || task.IsDeleted)
                return Results.NotFound();

            task.IsDeleted = true;
            await db.SaveChangesAsync();
            return Results.NoContent();
        });

        api.MapPost("/{id:long}/toggle", async (long id, AspireCRMDbContext db) =>
        {
            var task = await db.Set<CrmTask>().FindAsync(id);
            if (task is null || task.IsDeleted)
                return Results.NotFound();

            task.IsCompleted = !task.IsCompleted;
            task.CompletedAt = task.IsCompleted ? DateTime.UtcNow : null;
            await db.SaveChangesAsync();
            return Results.Ok(task);
        });
    }
}
