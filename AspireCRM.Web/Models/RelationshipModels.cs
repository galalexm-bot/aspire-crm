namespace AspireCRM.Web.Models;

public class RelationshipListResponse
{
    public List<RelationshipListItem> Items { get; set; } = [];
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
}

public class RelationshipListItem
{
    public long Id { get; set; }
    public string Theme { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public string RelationshipType { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool? Completed { get; set; }
    public string? ContractorName { get; set; }
    public string? SaleName { get; set; }
    public string? LeadName { get; set; }
    public string? ContactName { get; set; }
}

public class CreateRelationshipRequest
{
    public string Type { get; set; } = "Base";
    public string Theme { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Priority { get; set; } = "Medium";
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public long? ContractorId { get; set; }
    public long? SaleId { get; set; }
    public long? LeadId { get; set; }
    public long? ContactId { get; set; }
    public string? CallType { get; set; }
    public string? UniqueId { get; set; }
    public string? Place { get; set; }
    public bool TimeNotSet { get; set; }
    public bool IsPrivate { get; set; }
}

public class UpdateRelationshipRequest
{
    public string Theme { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Priority { get; set; } = "Medium";
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public long? ContractorId { get; set; }
    public long? SaleId { get; set; }
    public long? LeadId { get; set; }
    public long? ContactId { get; set; }
    public string? CallType { get; set; }
    public string? Place { get; set; }
    public bool TimeNotSet { get; set; }
    public bool IsPrivate { get; set; }
}
