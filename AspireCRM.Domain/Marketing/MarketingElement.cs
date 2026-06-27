using AspireCRM.Domain.Common;

namespace AspireCRM.Domain.Marketing;

public class MarketingElement : BaseEntity
{
    public long MarketingActivityId { get; set; }
    public MarketingActivity? MarketingActivity { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? ElementType { get; set; }
    public decimal? Cost { get; set; }
    public decimal? ExpectedRevenue { get; set; }
    public decimal? ActualRevenue { get; set; }
    public string? Description { get; set; }
}
