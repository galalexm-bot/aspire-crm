using AspireCRM.Domain.Contractors;
using AspireCRM.Domain.Leads;
using AspireCRM.Domain.Payments;
using AspireCRM.Domain.Relationships;
using AspireCRM.Domain.Sales;

namespace AspireCRM.Domain.Common;

public class Comment : BaseEntity
{
    public string Text { get; set; } = string.Empty;
    public long AuthorId { get; set; }
    public DateTime CreationDate { get; set; }

    public ICollection<Lead> Leads { get; set; } = new List<Lead>();
    public ICollection<Contractor> Contractors { get; set; } = new List<Contractor>();
    public ICollection<Sale> Sales { get; set; } = new List<Sale>();
    public ICollection<Inpayment> Inpayments { get; set; } = new List<Inpayment>();
    public ICollection<Relationship> Relationships { get; set; } = new List<Relationship>();
}