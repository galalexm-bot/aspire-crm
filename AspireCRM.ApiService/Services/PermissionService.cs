using System.Security.Claims;
using AspireCRM.DataLayer;
using AspireCRM.Domain.Security;
using Microsoft.EntityFrameworkCore;

namespace AspireCRM.ApiService.Services;

public interface IPermissionService
{
    long GetCurrentUserId();
    Task<bool> HasPermissionAsync(long userId, long categoryId, CategoryPermissionLevel requiredLevel);
    Task<Dictionary<long, CategoryPermissionLevel>> GetUserPermissionsAsync(long userId);
    Task<bool> HasAnyCategoryPermissionAsync(long userId, CategoryPermissionLevel requiredLevel);
}

public class PermissionService : IPermissionService
{
    private readonly AspireCRMDbContext _db;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public PermissionService(AspireCRMDbContext db, IHttpContextAccessor httpContextAccessor)
    {
        _db = db;
        _httpContextAccessor = httpContextAccessor;
    }

    public long GetCurrentUserId()
    {
        var claim = _httpContextAccessor.HttpContext?.User.FindFirst("userId")?.Value;
        return long.TryParse(claim, out var userId) ? userId : 0;
    }

    public async Task<bool> HasPermissionAsync(long userId, long categoryId, CategoryPermissionLevel requiredLevel)
    {
        if (requiredLevel == CategoryPermissionLevel.None) return true;

        var permission = await _db.UserCategoryPermissions
            .FirstOrDefaultAsync(p => p.UserId == userId && p.CategoryId == categoryId);

        return permission is not null && permission.PermissionLevel >= requiredLevel;
    }

    public async Task<Dictionary<long, CategoryPermissionLevel>> GetUserPermissionsAsync(long userId)
    {
        return await _db.UserCategoryPermissions
            .Where(p => p.UserId == userId)
            .ToDictionaryAsync(p => p.CategoryId, p => p.PermissionLevel);
    }

    public async Task<bool> HasAnyCategoryPermissionAsync(long userId, CategoryPermissionLevel requiredLevel)
    {
        return await _db.UserCategoryPermissions
            .AnyAsync(p => p.UserId == userId && p.PermissionLevel >= requiredLevel);
    }
}