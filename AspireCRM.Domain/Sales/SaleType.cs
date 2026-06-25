using AspireCRM.Domain.Common;

namespace AspireCRM.Domain.Sales;

public class SaleType : BaseEntity
{
    public string Name { get; set; } = string.Empty;
}