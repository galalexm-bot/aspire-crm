namespace AspireCRM.Web.Models;

public class LeadSummary
{
    public int TotalNew { get; set; }
    public int TotalInHand { get; set; }
    public int TotalQualified { get; set; }
    public int TotalUnqualified { get; set; }
    public int TotalDuplicate { get; set; }
    public int TotalConversationNotStart { get; set; }
    public int TotalLeads { get; set; }
}
