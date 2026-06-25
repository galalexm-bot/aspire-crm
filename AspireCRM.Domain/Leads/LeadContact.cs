using AspireCRM.Domain.Common;
using AspireCRM.Domain.Contractors;

namespace AspireCRM.Domain.Leads;

public class LeadContact : BaseEntity
{
    public long LeadId { get; set; }
    public Lead Lead { get; set; } = null!;
    public long ContactId { get; set; }
    public Contact Contact { get; set; } = null!;
}