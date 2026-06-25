using AspireCRM.Domain.Common;

namespace AspireCRM.Domain.Leads;

public class LeadType : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public ICollection<Lead> Leads { get; set; } = new List<Lead>();
}