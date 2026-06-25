using AspireCRM.Domain.Common;
using AspireCRM.Domain.Contractors;
using AspireCRM.Domain.Sales;

namespace AspireCRM.Domain.Payments;

public class Inpayment : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Comment { get; set; }
    public string? ChangeStatusComment { get; set; }
    public decimal Sum { get; set; }
    public long? CurrencyId { get; set; }
    public bool Invoice { get; set; }

    public InpaymentStatus Status { get; set; }

    public long SaleId { get; set; }
    public Sale Sale { get; set; } = null!;
    public long ContractorId { get; set; }
    public Contractor Contractor { get; set; } = null!;
    public long? ResponsibleId { get; set; }
    public long? CreationAuthorId { get; set; }
    public long? ChangeAuthorId { get; set; }

    public DateTime CreationDate { get; set; }
    public DateTime? ChangeDate { get; set; }
    public DateTime? Date { get; set; }
    public DateTime? ActualDate { get; set; }
    public DateTime? ChangeStatusDate { get; set; }

    public ICollection<Comment> Comments { get; set; } = new List<Comment>();
}