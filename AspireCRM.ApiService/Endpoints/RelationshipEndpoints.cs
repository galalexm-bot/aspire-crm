using AspireCRM.DataLayer.Repositories;
using AspireCRM.Domain.Relationships;

namespace AspireCRM.ApiService.Endpoints;

public static class RelationshipEndpoints
{
    public static void MapRelationshipEndpoints(this WebApplication app)
    {
        var api = app.MapGroup("/api/relationships");

        api.MapGet("/", async (IRepository<Relationship> repo, int page = 1, int pageSize = 20, long? leadId = null, long? contractorId = null, long? saleId = null) =>
        {
            System.Linq.Expressions.Expression<Func<Relationship, bool>>? filter = null;
            if (leadId.HasValue) filter = r => r.LeadId == leadId;
            else if (contractorId.HasValue) filter = r => r.ContractorId == contractorId;
            else if (saleId.HasValue) filter = r => r.SaleId == saleId;
            return Results.Ok(await repo.GetPagedAsync(page, pageSize, filter));
        });

        api.MapGet("/{id:long}", async (long id, IRepository<Relationship> repo) =>
        {
            var relationship = await repo.GetByIdAsync(id);
            return relationship is null ? Results.NotFound() : Results.Ok(relationship);
        });

        api.MapPost("/", async (Relationship relationship, IRepository<Relationship> repo) =>
        {
            var created = await repo.AddAsync(relationship);
            return Results.Created($"/api/relationships/{created.Id}", created);
        });

        api.MapPut("/{id:long}", async (long id, Relationship relationship, IRepository<Relationship> repo) =>
        {
            if (id != relationship.Id) return Results.BadRequest();
            await repo.UpdateAsync(relationship);
            return Results.NoContent();
        });

        api.MapDelete("/{id:long}", async (long id, IRepository<Relationship> repo) =>
        {
            var relationship = await repo.GetByIdAsync(id);
            if (relationship is null) return Results.NotFound();
            await repo.DeleteAsync(relationship);
            return Results.NoContent();
        });
    }
}