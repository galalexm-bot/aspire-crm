using AspireCRM.Domain.Marketing;

namespace AspireCRM.Web.Models;

public class CreateMarketingActivityRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public decimal? Budget { get; set; }
    public MarketingActivityStatus Status { get; set; } = MarketingActivityStatus.Planned;
    public MarketingActivityType Type { get; set; } = MarketingActivityType.Other;
    public string? MarketingEffect { get; set; }
}

public class UpdateMarketingActivityRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public decimal? Budget { get; set; }
    public MarketingActivityStatus Status { get; set; } = MarketingActivityStatus.Planned;
    public MarketingActivityType Type { get; set; } = MarketingActivityType.Other;
    public string? MarketingEffect { get; set; }
}

public class MarketingActivityStats
{
    public int LeadCount { get; set; }
    public int SaleCount { get; set; }
    public decimal? TotalBudget { get; set; }
    public decimal? TotalCost { get; set; }
    public decimal ExpectedRevenue { get; set; }
    public decimal ActualRevenue { get; set; }
}
