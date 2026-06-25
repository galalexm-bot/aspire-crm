using AspireCRM.Domain.Common;

namespace AspireCRM.Domain.Contractors;

public class ContractorIndustry : BaseEntity
{
    public string Name { get; set; } = string.Empty;
}