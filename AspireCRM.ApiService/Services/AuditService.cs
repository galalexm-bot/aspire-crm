using AspireCRM.DataLayer;
using AspireCRM.Domain.Audit;

namespace AspireCRM.ApiService.Services;

public class AuditService
{
    private readonly AspireCRMDbContext _db;
    private readonly ITenantService _tenantService;

    public AuditService(AspireCRMDbContext db, ITenantService tenantService)
    {
        _db = db;
        _tenantService = tenantService;
    }

    public async Task LogAsync(string entityType, long entityId, string field,
        string? oldValue, string? newValue, long changedById, string? comment = null)
    {
        if (!_tenantService.TenantId.HasValue)
            return;

        var log = new AuditLog
        {
            TenantId = _tenantService.TenantId.Value,
            EntityType = entityType,
            EntityId = entityId,
            Field = field,
            OldValue = oldValue,
            NewValue = newValue,
            ChangedById = changedById,
            ChangedAt = DateTime.UtcNow,
            Comment = comment
        };

        _db.AuditLogs.Add(log);
        await _db.SaveChangesAsync();
    }
}