using AspireCRM.Domain.Common;
using AspireCRM.Domain.Contractors;
using AspireCRM.Domain.Leads;
using AspireCRM.Domain.Sales;

namespace AspireCRM.Domain.Relationships;

public class Relationship : BaseEntity
{
    public string Theme { get; set; } = string.Empty;
    public string? Description { get; set; }

    public RelationshipPriority Priority { get; set; } = RelationshipPriority.Medium;
    public bool InheritPermissions { get; set; }
    public bool IsPrivate { get; set; }
    public bool? Completed { get; set; }
    public bool ExpiredNotificationSent { get; set; }

    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public DateTime? CreationDate { get; set; }
    public DateTime? ChangeDate { get; set; }
    public DateTime? DoneDate { get; set; }

    public long? CreationAuthorId { get; set; }
    public long? ChangeAuthorId { get; set; }
    public long? ContractorId { get; set; }
    public Contractor? Contractor { get; set; }
    public long? SaleId { get; set; }
    public Sale? Sale { get; set; }
    public long? ContactId { get; set; }
    public Contact? Contact { get; set; }
    public long? LeadId { get; set; }
    public Lead? Lead { get; set; }

    public ICollection<Comment> Comments { get; set; } = new List<Comment>();
    public ICollection<RelationshipUser> RelationshipUsers { get; set; } = new List<RelationshipUser>();
    public ICollection<Contact> ContractorsContacts { get; set; } = new List<Contact>();
    public ICollection<LeadContact> LeadContacts { get; set; } = new List<LeadContact>();
}