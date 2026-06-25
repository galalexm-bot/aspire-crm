using AspireCRM.Domain.Common;
using AspireCRM.Domain.Contractors;
using AspireCRM.Domain.Payments;
using AspireCRM.Domain.Relationships;

namespace AspireCRM.Domain.Sales;

public class Sale : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? ShortStatus { get; set; }
    public string? Description { get; set; }
    public string? MarketingEffect { get; set; }
    public double? SalesVolume { get; set; }
    public bool InpaymentsPlanCompleted { get; set; }

    public SalePriority Priority { get; set; } = SalePriority.Medium;
    public SaleStatus SaleStatus { get; set; }

    public long ContractorId { get; set; }
    public Contractor Contractor { get; set; } = null!;
    public long? CurrencyId { get; set; }
    public long ResponsibleId { get; set; }
    public long? AuthorId { get; set; }
    public long SaleTypeId { get; set; }
    public long? SaleStageId { get; set; }
    public long? RegionId { get; set; }
    public long? ContractorIndustryId { get; set; }
    public long? SaleFunnelId { get; set; }
    public long? PreviousStageId { get; set; }
    public long? StatusChangeCommentId { get; set; }
    public long? StageChangeCommentId { get; set; }
    public long? NextRelationshipId { get; set; }

    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public DateTime CreationDate { get; set; }
    public DateTime? StatusChangeDate { get; set; }
    public DateTime? StageChangeDate { get; set; }

    public ICollection<Comment> Comments { get; set; } = new List<Comment>();
    public ICollection<Relationship> Relationships { get; set; } = new List<Relationship>();
    public ICollection<Inpayment> Inpayments { get; set; } = new List<Inpayment>();
    public ICollection<SaleProduct> Products { get; set; } = new List<SaleProduct>();
}