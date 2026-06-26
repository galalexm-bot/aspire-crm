using AspireCRM.DataLayer.Repositories;
using AspireCRM.Domain.Leads;

namespace AspireCRM.ApiService.Endpoints;

public static class LeadEndpoints
{
    public static void MapLeadEndpoints(this WebApplication app)
    {
        var api = app.MapGroup("/api/leads");

        api.MapGet("/", async (IRepository<Lead> repo, int page = 1, int pageSize = 20, LeadStatus? status = null) =>
        {
            var filter = status.HasValue ? (System.Linq.Expressions.Expression<Func<Lead, bool>>)(l => l.Status == status.Value) : null;
            return Results.Ok(await repo.GetPagedAsync(page, pageSize, filter));
        });

        api.MapGet("/{id:long}", async (long id, IRepository<Lead> repo) =>
        {
            var lead = await repo.GetByIdAsync(id);
            return lead is null ? Results.NotFound() : Results.Ok(lead);
        });

        api.MapPost("/", async (Lead lead, IRepository<Lead> repo) =>
        {
            var created = await repo.AddAsync(lead);
            return Results.Created($"/api/leads/{created.Id}", created);
        });

        api.MapPut("/{id:long}", async (long id, Lead lead, IRepository<Lead> repo) =>
        {
            if (id != lead.Id) return Results.BadRequest();
            await repo.UpdateAsync(lead);
            return Results.NoContent();
        });

        api.MapDelete("/{id:long}", async (long id, IRepository<Lead> repo) =>
        {
            var lead = await repo.GetByIdAsync(id);
            if (lead is null) return Results.NotFound();
            await repo.DeleteAsync(lead);
            return Results.NoContent();
        });
    }
}