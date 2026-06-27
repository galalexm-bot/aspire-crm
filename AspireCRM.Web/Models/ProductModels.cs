namespace AspireCRM.Web.Models;

public class ProductTreeNode
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsGroup { get; set; }
    public double? Price { get; set; }
    public long? ParentId { get; set; }
    public List<ProductTreeNode> Children { get; set; } = [];
}

public class CreateProductRequest
{
    public long? ParentId { get; set; }
    public bool IsGroup { get; set; }
    public string Name { get; set; } = string.Empty;
    public double? Price { get; set; }
}

public class UpdateProductRequest
{
    public string Name { get; set; } = string.Empty;
    public double? Price { get; set; }
}

public class MoveProductRequest
{
    public long? ParentId { get; set; }
}