using AspireCRM.DataLayer.Repositories;
using AspireCRM.Domain.Products;

namespace AspireCRM.ApiService.Endpoints;

public static class ProductEndpoints
{
    public static void MapProductEndpoints(this WebApplication app)
    {
        var api = app.MapGroup("/api/products");

        api.MapGet("/", async (IRepository<Product> repo) =>
            Results.Ok(await repo.GetAllAsync()));

        api.MapGet("/{id:long}", async (long id, IRepository<Product> repo) =>
        {
            var product = await repo.GetByIdAsync(id);
            return product is null ? Results.NotFound() : Results.Ok(product);
        });

        api.MapPost("/", async (Product product, IRepository<Product> repo) =>
        {
            var created = await repo.AddAsync(product);
            return Results.Created($"/api/products/{created.Id}", created);
        });

        api.MapPut("/{id:long}", async (long id, Product product, IRepository<Product> repo) =>
        {
            if (id != product.Id) return Results.BadRequest();
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
}