namespace AspireCRM.Web.Models;

public class SaleSummary
{
    public int TotalActive { get; set; }
    public int TotalPostponed { get; set; }
    public int TotalPositiveClosed { get; set; }
    public int TotalNegativeClosed { get; set; }
    public double TotalSalesVolume { get; set; }
    public double ActiveSalesVolume { get; set; }
    public int OverdueCount { get; set; }
}