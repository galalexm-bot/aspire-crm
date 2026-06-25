using AspireCRM.Domain.Products;

using AspireCRM.Domain.Common;

namespace AspireCRM.Domain.Sales;

public class SaleProduct : BaseEntity
{
    public long SaleId { get; set; }
    public Sale Sale { get; set; } = null!;
    public long ProductId { get; set; }
    public Product Product { get; set; } = null!;
    public double Quantity { get; set; }
    public double Price { get; set; }
    public double? Discount { get; set; }
}