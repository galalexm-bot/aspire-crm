using AspireCRM.DataLayer.Repositories;
using AspireCRM.Domain.Sales;

namespace AspireCRM.ApiService.Endpoints;

public static class SaleLookupEndpoints
{
    public static void MapSaleLookupEndpoints(this WebApplication app)
    {
        var funnels = app.MapGroup("/api/sale-funnels");
        funnels.MapGet("/", async (IRepository<SaleFunnel> repo) =>
            Results.Ok(await repo.GetAllAsync()));
        funnels.MapPost("/", async (SaleFunnel entity, IRepository<SaleFunnel> repo) =>
        {
            var created = await repo.AddAsync(entity);
            return Results.Created($"/api/sale-funnels/{created.Id}", created);
        });
        funnels.MapPut("/{id:long}", async (long id, SaleFunnel entity, IRepository<SaleFunnel> repo) =>
        {
            if (id != entity.Id) return Results.BadRequest();
            await repo.UpdateAsync(entity);
            return Results.NoContent();
        });
        funnels.MapDelete("/{id:long}", async (long id, IRepository<SaleFunnel> repo) =>
        {
            var entity = await repo.GetByIdAsync(id);
            if (entity is null) return Results.NotFound();
            await repo.DeleteAsync(entity);
            return Results.NoContent();
        });

        var stages = app.MapGroup("/api/sale-stages");
        stages.MapGet("/", async (IRepository<SaleStage> repo) =>
            Results.Ok(await repo.GetAllAsync()));
        stages.MapPost("/", async (SaleStage entity, IRepository<SaleStage> repo) =>
        {
            var created = await repo.AddAsync(entity);
            return Results.Created($"/api/sale-stages/{created.Id}", created);
        });
        stages.MapPut("/{id:long}", async (long id, SaleStage entity, IRepository<SaleStage> repo) =>
        {
            if (id != entity.Id) return Results.BadRequest();
            await repo.UpdateAsync(entity);
            return Results.NoContent();
        });
        stages.MapDelete("/{id:long}", async (long id, IRepository<SaleStage> repo) =>
        {
            var entity = await repo.GetByIdAsync(id);
            if (entity is null) return Results.NotFound();
            await repo.DeleteAsync(entity);
            return Results.NoContent();
        });
        stages.MapPut("/reorder", async (List<SaleStage> items, IRepository<SaleStage> repo) =>
        {
            foreach (var item in items)
            {
                var existing = await repo.GetByIdAsync(item.Id);
                if (existing is not null)
                {
                    existing.SortOrder = item.SortOrder;
                    await repo.UpdateAsync(existing);
                }
            }
            return Results.NoContent();
        });

        var types = app.MapGroup("/api/sale-types");
        types.MapGet("/", async (IRepository<SaleType> repo) =>
            Results.Ok(await repo.GetAllAsync()));
        types.MapPost("/", async (SaleType entity, IRepository<SaleType> repo) =>
        {
            var created = await repo.AddAsync(entity);
            return Results.Created($"/api/sale-types/{created.Id}", created);
        });
        types.MapPut("/{id:long}", async (long id, SaleType entity, IRepository<SaleType> repo) =>
        {
            if (id != entity.Id) return Results.BadRequest();
            await repo.UpdateAsync(entity);
            return Results.NoContent();
        });
        types.MapDelete("/{id:long}", async (long id, IRepository<SaleType> repo) =>
        {
            var entity = await repo.GetByIdAsync(id);
            if (entity is null) return Results.NotFound();
            await repo.DeleteAsync(entity);
            return Results.NoContent();
        });
    }
}