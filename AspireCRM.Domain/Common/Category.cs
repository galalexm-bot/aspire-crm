using AspireCRM.Domain.Contractors;
using AspireCRM.Domain.Leads;

namespace AspireCRM.Domain.Common;

public class Category : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public CategoryType CategoryType { get; set; } = CategoryType.Normal;

    public ICollection<Lead> Leads { get; set; } = new List<Lead>();
    public ICollection<Contractor> Contractors { get; set; } = new List<Contractor>();
}