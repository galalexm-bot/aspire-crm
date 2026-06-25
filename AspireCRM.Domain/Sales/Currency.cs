using AspireCRM.Domain.Common;

namespace AspireCRM.Domain.Sales;

public class Currency : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
}