using AspireCRM.Domain.Common;

namespace AspireCRM.Domain.Sales;

public class SalesPlan : BaseEntity
{
    public int Year { get; set; }
    public int Month { get; set; }
    public decimal PlannedAmount { get; set; }
    public decimal? ActualAmount { get; set; }
    public string? Description { get; set; }
}