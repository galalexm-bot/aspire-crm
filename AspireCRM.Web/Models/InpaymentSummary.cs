namespace AspireCRM.Web.Models;

public class InpaymentSummary
{
    public double PlanSum { get; set; }
    public double ReceivedSum { get; set; }
    public double CancelledSum { get; set; }
    public int OverdueCount { get; set; }
    public int TotalCount { get; set; }
    public int PlanCount { get; set; }
    public int ReceivedCount { get; set; }
}
