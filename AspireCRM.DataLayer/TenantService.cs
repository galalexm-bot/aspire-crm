namespace AspireCRM.DataLayer;

public interface ITenantService
{
    long? TenantId { get; }
    void SetTenantId(long tenantId);
}

public class TenantService : ITenantService
{
    public long? TenantId { get; private set; }
    public void SetTenantId(long tenantId) => TenantId = tenantId;
}