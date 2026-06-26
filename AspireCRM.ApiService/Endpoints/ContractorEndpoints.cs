using AspireCRM.DataLayer.Repositories;
using AspireCRM.Domain.Contractors;

namespace AspireCRM.ApiService.Endpoints;

public static class ContractorEndpoints
{
    public static void MapContractorEndpoints(this WebApplication app)
    {
        var api = app.MapGroup("/api/contractors");

        api.MapGet("/", async (IRepository<Contractor> repo, int page = 1, int pageSize = 20) =>
            Results.Ok(await repo.GetPagedAsync(page, pageSize)));

        api.MapGet("/{id:long}", async (long id, IRepository<Contractor> repo) =>
        {
            var contractor = await repo.GetByIdAsync(id);
            return contractor is null ? Results.NotFound() : Results.Ok(contractor);
        });

        api.MapPost("/", async (Contractor contractor, IRepository<Contractor> repo) =>
        {
            var created = await repo.AddAsync(contractor);
            return Results.Created($"/api/contractors/{created.Id}", created);
        });

        api.MapPut("/{id:long}", async (long id, Contractor contractor, IRepository<Contractor> repo) =>
        {
            if (id != contractor.Id) return Results.BadRequest();
            await repo.UpdateAsync(contractor);
            return Results.NoContent();
        });

        api.MapDelete("/{id:long}", async (long id, IRepository<Contractor> repo) =>
        {
            var contractor = await repo.GetByIdAsync(id);
            if (contractor is null) return Results.NotFound();
            await repo.DeleteAsync(contractor);
            return Results.NoContent();
        });
    }
}