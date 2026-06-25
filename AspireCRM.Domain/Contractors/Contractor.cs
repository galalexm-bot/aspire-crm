using AspireCRM.Domain.Common;
using AspireCRM.Domain.Leads;
using AspireCRM.Domain.Payments;
using AspireCRM.Domain.Relationships;
using AspireCRM.Domain.Sales;

namespace AspireCRM.Domain.Contractors;

public class Contractor : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Fax { get; set; }
    public string? Site { get; set; }
    public string? INN { get; set; }
    public double? AnnualIncome { get; set; }
    public DateTime? CompanyDay { get; set; }

    public long? ResponsibleId { get; set; }
    public long? CreationAuthorId { get; set; }
    public long? ChangeAuthorId { get; set; }
    public long? LegalAddressId { get; set; }
    public Address? LegalAddress { get; set; }
    public long? PostalAddressId { get; set; }
    public Address? PostalAddress { get; set; }
    public long? RegionId { get; set; }
    public long? IndustryId { get; set; }
    public long? PartnerId { get; set; }
    public Contractor? Partner { get; set; }
    public long? ContractorTypeId { get; set; }
    public long? NextRelationshipId { get; set; }

    public DateTime? CreationDate { get; set; }
    public DateTime? ChangeDate { get; set; }

    public ICollection<Email> Emails { get; set; } = new List<Email>();
    public ICollection<Phone> Phones { get; set; } = new List<Phone>();
    public ICollection<Comment> Comments { get; set; } = new List<Comment>();
    public ICollection<Category> Categories { get; set; } = new List<Category>();
    public ICollection<Contact> Contacts { get; set; } = new List<Contact>();
    public ICollection<Relationship> Relationships { get; set; } = new List<Relationship>();
    public ICollection<Lead> Leads { get; set; } = new List<Lead>();
    public ICollection<Sale> Sales { get; set; } = new List<Sale>();
    public ICollection<Inpayment> Inpayments { get; set; } = new List<Inpayment>();
}