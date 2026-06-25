namespace AspireCRM.Domain.Relationships;

public class RelationshipMeeting : Relationship
{
    public string? Place { get; set; }
    public bool TimeNotSet { get; set; }
}