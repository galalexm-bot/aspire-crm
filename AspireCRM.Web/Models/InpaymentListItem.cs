using AspireCRM.Domain.Payments;

namespace AspireCRM.Web.Models;

public class InpaymentListItem
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Sum { get; set; }
    public InpaymentStatus Status { get; set; }
    public DateTime? Date { get; set; }
    public DateTime CreationDate { get; set; }
    public long SaleId { get; set; }
    public string SaleName { get; set; } = string.Empty;
    public long ContractorId { get; set; }
    public string ContractorName { get; set; } = string.Empty;
}