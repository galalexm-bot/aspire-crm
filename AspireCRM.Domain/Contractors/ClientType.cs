using AspireCRM.Domain.Common;

namespace AspireCRM.Domain.Contractors;

public class ClientType : BaseEntity
{
    public string Name { get; set; } = string.Empty;
}