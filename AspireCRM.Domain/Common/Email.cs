using AspireCRM.Domain.Contractors;
using AspireCRM.Domain.Leads;

namespace AspireCRM.Domain.Common;

public class Email : BaseEntity
{
    public string EmailAddress { get; set; } = string.Empty;
    public string? Description { get; set; }

    public ICollection<Lead> Leads { get; set; } = new List<Lead>();
    public ICollection<Contractor> Contractors { get; set; } = new List<Contractor>();
}