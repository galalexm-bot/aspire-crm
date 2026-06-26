using AspireCRM.DataLayer.Repositories;
using AspireCRM.Domain.Sales;

namespace AspireCRM.ApiService.Endpoints;

public static class SaleEndpoints
{
    public static void MapSaleEndpoints(this WebApplication app)
    {
        var api = app.MapGroup("/api/sales");

        api.MapGet("/", async (IRepository<Sale> repo, int page = 1, int pageSize = 20) =>
            Results.Ok(await repo.GetPagedAsync(page, pageSize)));

        api.MapGet("/{id:long}", async (long id, IRepository<Sale> repo) =>
        {
            var sale = await repo.GetByIdAsync(id);
            return sale is null ? Results.NotFound() : Results.Ok(sale);
        });

        api.MapPost("/", async (Sale sale, IRepository<Sale> repo) =>
        {
            var created = await repo.AddAsync(sale);
            return Results.Created($"/api/sales/{created.Id}", created);
        });

        api.MapPut("/{id:long}", async (long id, Sale sale, IRepository<Sale> repo) =>
        {
            if (id != sale.Id) return Results.BadRequest();
            await repo.UpdateAsync(sale);
            return Results.NoContent();
        });

        api.MapDelete("/{id:long}", async (long id, IRepository<Sale> repo) =>
        {
            var sale = await repo.GetByIdAsync(id);
            if (sale is null) return Results.NotFound();
            await repo.DeleteAsync(sale);
            return Results.NoContent();
        });
    }
}