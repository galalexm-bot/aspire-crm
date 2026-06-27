using AspireCRM.Domain.Common;

namespace AspireCRM.Domain.Marketing;

public class MarketingActivity : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public decimal? Budget { get; set; }
    public decimal? ActualCost { get; set; }
    public MarketingActivityStatus Status { get; set; } = MarketingActivityStatus.Planned;
    public MarketingActivityType Type { get; set; } = MarketingActivityType.Other;
    public string? MarketingEffect { get; set; }

    public ICollection<MarketingPayment> Payments { get; set; } = new List<MarketingPayment>();
    public ICollection<MarketingElement> Elements { get; set; } = new List<MarketingElement>();
}
