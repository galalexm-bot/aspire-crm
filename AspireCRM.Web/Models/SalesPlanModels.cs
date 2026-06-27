namespace AspireCRM.Web.Models;

public class CreateSalesPlanRequest
{
    public int Year { get; set; }
    public int Month { get; set; }
    public decimal PlannedAmount { get; set; }
    public decimal? ActualAmount { get; set; }
    public string? Description { get; set; }
}

public class UpdateSalesPlanRequest
{
    public int Year { get; set; }
    public int Month { get; set; }
    public decimal PlannedAmount { get; set; }
    public decimal? ActualAmount { get; set; }
    public string? Description { get; set; }
}