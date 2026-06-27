using AspireCRM.DataLayer;
using Microsoft.EntityFrameworkCore;

namespace AspireCRM.ApiService.Endpoints;

public static class AuditLogEndpoints
{
    public static void MapAuditLogEndpoints(this WebApplication app)
    {
        var api = app.MapGroup("/api/audit");

        api.MapGet("/{entityType}/{entityId:long}", async (string entityType, long entityId,
            AspireCRMDbContext db, ITenantService tenantService) =>
        {
            if (!tenantService.TenantId.HasValue)
                return Results.Unauthorized();

            var logs = await db.AuditLogs
                .Where(a => a.TenantId == tenantService.TenantId.Value
                    && a.EntityType == entityType
                    && a.EntityId == entityId)
                .OrderByDescending(a => a.ChangedAt)
                .Select(a => new AuditLogDto
                {
                    Id = a.Id,
                    Field = a.Field,
                    OldValue = a.OldValue,
                    NewValue = a.NewValue,
                    ChangedById = a.ChangedById,
                    ChangedAt = a.ChangedAt,
                    Comment = a.Comment,
                    ChangedByName = ""
                })
                .ToListAsync();

            var userIds = logs.Select(l => l.ChangedById).Distinct().ToList();
            var users = await db.Users
                .Where(u => userIds.Contains(u.Id))
                .Select(u => new { u.Id, Name = u.FirstName + " " + u.LastName })
                .ToListAsync();

            var userNames = users.ToDictionary(u => u.Id, u => u.Name.Trim());

            foreach (var l in logs)
                l.ChangedByName = userNames.GetValueOrDefault(l.ChangedById, $"#{l.ChangedById}");

            return Results.Ok(logs);
        });
    }
}

public class AuditLogDto
{
    public long Id { get; set; }
    public string Field { get; set; } = string.Empty;
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
    public long ChangedById { get; set; }
    public string ChangedByName { get; set; } = string.Empty;
    public DateTime ChangedAt { get; set; }
    public string? Comment { get; set; }
}