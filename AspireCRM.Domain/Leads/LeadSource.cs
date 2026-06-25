using AspireCRM.Domain.Common;

namespace AspireCRM.Domain.Leads;

public class LeadSource : BaseEntity
{
    public string Name { get; set; } = string.Empty;
}