using AspireCRM.DataLayer;
using AspireCRM.DataLayer.Repositories;
using AspireCRM.Domain.Common;
using AspireCRM.Domain.Security;
using Microsoft.EntityFrameworkCore;

namespace AspireCRM.ApiService.Endpoints;

public static class SecurityEndpoints
{
    public static void MapSecurityEndpoints(this WebApplication app)
    {
        var api = app.MapGroup("/api/security");

        api.MapGet("/users", async (AspireCRMDbContext db) =>
        {
            var users = await db.Users
                .Select(u => new { u.Id, u.UserName, u.Email, u.FirstName, u.LastName, u.IsActive })
                .ToListAsync();
            return Results.Ok(users);
        });

        api.MapGet("/permissions", async (IRepository<UserCategoryPermission> repo) =>
            Results.Ok(await repo.GetAllAsync()));

        api.MapGet("/permissions/by-user/{userId:long}", async (long userId, AspireCRMDbContext db) =>
        {
            var permissions = await db.UserCategoryPermissions
                .Include(p => p.Category)
                .Where(p => p.UserId == userId)
                .ToListAsync();
            return Results.Ok(permissions);
        });

        api.MapGet("/permissions/by-category/{categoryId:long}", async (long categoryId, AspireCRMDbContext db) =>
        {
            var permissions = await db.UserCategoryPermissions
                .Where(p => p.CategoryId == categoryId)
                .ToListAsync();
            return Results.Ok(permissions);
        });

        api.MapPost("/permissions", async (SetPermissionRequest request, IRepository<UserCategoryPermission> repo, AspireCRMDbContext db) =>
        {
            var existing = await db.UserCategoryPermissions
                .FirstOrDefaultAsync(p => p.UserId == request.UserId && p.CategoryId == request.CategoryId);

            if (existing is not null)
            {
                existing.PermissionLevel = request.PermissionLevel;
                await repo.UpdateAsync(existing);
                return Results.Ok(existing);
            }

            var permission = new UserCategoryPermission
            {
                UserId = request.UserId,
                CategoryId = request.CategoryId,
                PermissionLevel = request.PermissionLevel
            };
            var created = await repo.AddAsync(permission);
            return Results.Created($"/api/security/permissions/{created.Id}", created);
        });

        api.MapDelete("/permissions/{id:long}", async (long id, IRepository<UserCategoryPermission> repo) =>
        {
            var permission = await repo.GetByIdAsync(id);
            if (permission is null) return Results.NotFound();
            await repo.DeleteAsync(permission);
            return Results.NoContent();
        });
    }
}

public record SetPermissionRequest(long UserId, long CategoryId, CategoryPermissionLevel PermissionLevel);