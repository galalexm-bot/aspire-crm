using AspireCRM.Domain.Common;

namespace AspireCRM.Domain.Products;

public class Product : BaseEntity
{
    public long? ParentId { get; set; }
    public Product? Parent { get; set; }
    public ICollection<Product> Children { get; set; } = new List<Product>();

    public bool IsGroup { get; set; }
    public string Name { get; set; } = string.Empty;
    public double? Price { get; set; }
}