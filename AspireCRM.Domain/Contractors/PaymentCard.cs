using AspireCRM.Domain.Common;

namespace AspireCRM.Domain.Contractors;

public class PaymentCard : BaseEntity
{
    public string Number { get; set; } = string.Empty;
    public string? CardholderName { get; set; }
    public string? Description { get; set; }

    public long ContractorId { get; set; }
    public Contractor Contractor { get; set; } = null!;
}