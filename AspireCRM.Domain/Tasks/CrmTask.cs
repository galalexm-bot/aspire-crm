using AspireCRM.Domain.Common;

namespace AspireCRM.Domain.Tasks;

public class CrmTask : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime? DueDate { get; set; }
    public bool IsCompleted { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string EntityType { get; set; } = string.Empty;
    public long EntityId { get; set; }
    public long? AssignedToId { get; set; }
}
