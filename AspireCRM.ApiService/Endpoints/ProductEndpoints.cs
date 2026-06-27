using AspireCRM.DataLayer;
using AspireCRM.DataLayer.Repositories;
using AspireCRM.Domain.Products;
using Microsoft.EntityFrameworkCore;

namespace AspireCRM.ApiService.Endpoints;

public static class ProductEndpoints
{
    public static void MapProductEndpoints(this WebApplication app)
    {
        var api = app.MapGroup("/api/products");

        api.MapGet("/", async (IRepository<Product> repo) =>
            Results.Ok(await repo.GetAllAsync()));

        api.MapGet("/tree", async (AspireCRMDbContext db) =>
        {
            var products = await db.Products
                .Where(p => !p.IsDeleted)
                .OrderBy(p => p.Name)
                .ToListAsync();

            var roots = BuildTree(products, null);
            return Results.Ok(roots);
        });

        api.MapGet("/{id:long}", async (long id, IRepository<Product> repo) =>
        {
            var product = await repo.GetByIdAsync(id);
            return product is null ? Results.NotFound() : Results.Ok(product);
        });

        api.MapPost("/", async (CreateProductRequest request, IRepository<Product> repo) =>
        {
            var product = new Product
            {
                ParentId = request.ParentId,
                IsGroup = request.IsGroup,
                Name = request.Name.Trim(),
                Price = request.IsGroup ? null : request.Price
            };
            var created = await repo.AddAsync(product);
            return Results.Created($"/api/products/{created.Id}", created);
        });

        api.MapPut("/{id:long}", async (long id, UpdateProductRequest request, IRepository<Product> repo) =>
        {
            var product = await repo.GetByIdAsync(id);
            if (product is null) return Results.NotFound();

            product.Name = request.Name.Trim();
            product.Price = product.IsGroup ? null : request.Price;
            await repo.UpdateAsync(product);
            return Results.NoContent();
        });

        api.MapPut("/{id:long}/move", async (long id, MoveProductRequest request, IRepository<Product> repo) =>
        {
            var product = await repo.GetByIdAsync(id);
            if (product is null) return Results.NotFound();

            product.ParentId = request.ParentId;
            await repo.UpdateAsync(product);
            return Results.NoContent();
        });

        api.MapDelete("/{id:long}", async (long id, IRepository<Product> repo) =>
        {
            var product = await repo.GetByIdAsync(id);
            if (product is null) return Results.NotFound();
            await repo.DeleteAsync(product);
            return Results.NoContent();
        });
    }

    private static List<ProductTreeNode> BuildTree(List<Product> allProducts, long? parentId)
    {
        return allProducts
            .Where(p => p.ParentId == parentId)
            .Select(p => new ProductTreeNode
            {
                Id = p.Id,
                Name = p.Name,
                IsGroup = p.IsGroup,
                Price = p.Price,
                ParentId = p.ParentId,
                Children = BuildTree(allProducts, p.Id)
            })
            .ToList();
    }
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

public class ProductTreeNode
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsGroup { get; set; }
    public double? Price { get; set; }
    public long? ParentId { get; set; }
    public List<ProductTreeNode> Children { get; set; } = [];
}