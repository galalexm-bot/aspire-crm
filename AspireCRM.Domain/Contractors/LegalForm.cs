using AspireCRM.Domain.Common;

namespace AspireCRM.Domain.Contractors;

public class LegalForm : BaseEntity
{
    public string Name { get; set; } = string.Empty;
}