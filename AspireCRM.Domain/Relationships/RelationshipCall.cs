namespace AspireCRM.Domain.Relationships;

public class RelationshipCall : Relationship
{
    public RelationshipCallType Type { get; set; }
    public string? UniqueId { get; set; }
}