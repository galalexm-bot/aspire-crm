namespace AspireCRM.Domain.Common;

public abstract class BaseEntity
{
    public long Id { get; set; }
    public long TenantId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public long CreatedById { get; set; }
    public long? UpdatedById { get; set; }
    public bool IsDeleted { get; set; }
}