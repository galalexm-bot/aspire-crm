using AspireCRM.Domain.Common;

namespace AspireCRM.Domain.Relationships;

public class RelationshipUser : BaseEntity
{
    public long RelationshipId { get; set; }
    public Relationship Relationship { get; set; } = null!;
    public long UserId { get; set; }
    public RelationshipUserStatus Status { get; set; }
}