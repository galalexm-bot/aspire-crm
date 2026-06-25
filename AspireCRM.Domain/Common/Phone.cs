using AspireCRM.Domain.Contractors;
using AspireCRM.Domain.Leads;

namespace AspireCRM.Domain.Common;

public class Phone : BaseEntity
{
    public string PhoneNumber { get; set; } = string.Empty;
    public string? Description { get; set; }

    public ICollection<Lead> Leads { get; set; } = new List<Lead>();
    public ICollection<Contractor> Contractors { get; set; } = new List<Contractor>();
}