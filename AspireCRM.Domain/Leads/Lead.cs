using AspireCRM.Domain.Common;
using AspireCRM.Domain.Contractors;
using AspireCRM.Domain.Relationships;

namespace AspireCRM.Domain.Leads;

public class Lead : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Site { get; set; }
    public string? DublicateLead { get; set; }
    public string? DublicateContractor { get; set; }
    public string? DublicateSale { get; set; }
    public string? DublicateComment { get; set; }
    public string? MarketingEffect { get; set; }
    public string? UtmSource { get; set; }
    public string? UtmMedium { get; set; }
    public string? UtmTerm { get; set; }
    public string? UtmContent { get; set; }
    public string? UtmCampaign { get; set; }

    public LeadStatus Status { get; set; } = LeadStatus.New;

    public long? ResponsibleId { get; set; }
    public long? CreationAuthorId { get; set; }
    public long? ChangeAuthorId { get; set; }
    public long? SourceId { get; set; }
    public long? LegalFormId { get; set; }
    public long? AddressId { get; set; }
    public Address? Address { get; set; }
    public long? RegionId { get; set; }
    public long? IndustryId { get; set; }
    public long? ContractorId { get; set; }
    public Contractor? Contractor { get; set; }
    public long? SaleId { get; set; }
    public long? ConvertCommentId { get; set; }

    public DateTime? CreationDate { get; set; }
    public DateTime? ChangeDate { get; set; }
    public DateTime? InHandDate { get; set; }
    public DateTime? ConvertDate { get; set; }

    public ICollection<Email> Emails { get; set; } = new List<Email>();
    public ICollection<Phone> Phones { get; set; } = new List<Phone>();
    public ICollection<Comment> Comments { get; set; } = new List<Comment>();
    public ICollection<Category> Categories { get; set; } = new List<Category>();
    public ICollection<LeadContact> Contacts { get; set; } = new List<LeadContact>();
    public ICollection<Relationship> Relationships { get; set; } = new List<Relationship>();
}