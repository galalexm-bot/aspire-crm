using AspireCRM.Domain.Common;

namespace AspireCRM.Domain.Contractors;

public class BankAccount : BaseEntity
{
    public string Number { get; set; } = string.Empty;
    public string? BIK { get; set; }
    public string? BankName { get; set; }
    public string? CorrespondentAccount { get; set; }
    public string? Description { get; set; }

    public long ContractorId { get; set; }
    public Contractor Contractor { get; set; } = null!;
}