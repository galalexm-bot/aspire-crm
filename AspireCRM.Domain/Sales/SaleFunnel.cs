using AspireCRM.Domain.Common;

namespace AspireCRM.Domain.Sales;

public class SaleFunnel : BaseEntity
{
    public string Name { get; set; } = string.Empty;
}