using AspireCRM.Domain.Common;

namespace AspireCRM.Domain.Marketing;

public class MarketingPayment : BaseEntity
{
    public long MarketingActivityId { get; set; }
    public MarketingActivity? MarketingActivity { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime? PaymentDate { get; set; }
    public string? Description { get; set; }
}
