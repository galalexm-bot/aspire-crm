using AspireCRM.Domain.Common;

namespace AspireCRM.Domain.Contractors;

public class ClientDocumentType : BaseEntity
{
    public string Name { get; set; } = string.Empty;
}