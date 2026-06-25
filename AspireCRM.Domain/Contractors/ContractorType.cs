using AspireCRM.Domain.Common;

namespace AspireCRM.Domain.Contractors;

public class ContractorType : BaseEntity
{
    public string Name { get; set; } = string.Empty;
}