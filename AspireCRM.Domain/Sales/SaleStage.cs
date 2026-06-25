using AspireCRM.Domain.Common;

namespace AspireCRM.Domain.Sales;

public class SaleStage : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public int SortOrder { get; set; }
}