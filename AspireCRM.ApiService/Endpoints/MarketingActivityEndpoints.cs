using AspireCRM.DataLayer;
using AspireCRM.DataLayer.Repositories;
using AspireCRM.Domain.Marketing;
using Microsoft.EntityFrameworkCore;

namespace AspireCRM.ApiService.Endpoints;

public static class MarketingActivityEndpoints
{
    public static void MapMarketingActivityEndpoints(this WebApplication app)
    {
        var api = app.MapGroup("/api/marketing-activities");

        api.MapGet("/", async (IRepository<MarketingActivity> repo) =>
            Results.Ok(await repo.GetAllAsync()));

        api.MapGet("/{id:long}", async (long id, IRepository<MarketingActivity> repo) =>
        {
            var activity = await repo.GetByIdAsync(id);
            return activity is null ? Results.NotFound() : Results.Ok(activity);
        });

        api.MapPost("/", async (CreateMarketingActivityRequest request, IRepository<MarketingActivity> repo) =>
        {
            var activity = new MarketingActivity
            {
                Name = request.Name.Trim(),
                Description = request.Description?.Trim(),
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                Budget = request.Budget,
                Status = request.Status,
                Type = request.Type,
                MarketingEffect = request.MarketingEffect?.Trim()
            };
            var created = await repo.AddAsync(activity);
            return Results.Created($"/api/marketing-activities/{created.Id}", created);
        });

        api.MapPut("/{id:long}", async (long id, UpdateMarketingActivityRequest request, IRepository<MarketingActivity> repo) =>
        {
            var activity = await repo.GetByIdAsync(id);
            if (activity is null) return Results.NotFound();

            activity.Name = request.Name.Trim();
            activity.Description = request.Description?.Trim();
            activity.StartDate = request.StartDate;
            activity.EndDate = request.EndDate;
            activity.Budget = request.Budget;
            activity.Status = request.Status;
            activity.Type = request.Type;
            activity.MarketingEffect = request.MarketingEffect?.Trim();
            await repo.UpdateAsync(activity);
            return Results.NoContent();
        });

        api.MapDelete("/{id:long}", async (long id, IRepository<MarketingActivity> repo) =>
        {
            var activity = await repo.GetByIdAsync(id);
            if (activity is null) return Results.NotFound();
            await repo.DeleteAsync(activity);
            return Results.NoContent();
        });

        api.MapGet("/{id:long}/stats", async (long id, AspireCRMDbContext db,
            IRepository<Domain.Leads.Lead> leadRepo,
            IRepository<Domain.Sales.Sale> saleRepo) =>
        {
            var activity = await db.Set<MarketingActivity>()
                .Include(a => a.Elements)
                .Include(a => a.Payments)
                .FirstOrDefaultAsync(a => a.Id == id);
            if (activity is null) return Results.NotFound();

            var leads = await leadRepo.FindAsync(l => l.MarketingEffect == activity.MarketingEffect && l.MarketingEffect != null);
            var sales = await saleRepo.FindAsync(s => s.MarketingEffect == activity.MarketingEffect && s.MarketingEffect != null);

            return Results.Ok(new MarketingActivityStats
            {
                LeadCount = leads.Count,
                SaleCount = sales.Count,
                TotalBudget = activity.Budget,
                TotalCost = activity.ActualCost,
                ExpectedRevenue = activity.Elements.Sum(e => e.ExpectedRevenue ?? 0),
                ActualRevenue = activity.Elements.Sum(e => e.ActualRevenue ?? 0)
            });
        });
    }
}

public record CreateMarketingActivityRequest(
    string Name,
    string? Description,
    DateTime? StartDate,
    DateTime? EndDate,
    decimal? Budget,
    MarketingActivityStatus Status,
    MarketingActivityType Type,
    string? MarketingEffect
);

public record UpdateMarketingActivityRequest(
    string Name,
    string? Description,
    DateTime? StartDate,
    DateTime? EndDate,
    decimal? Budget,
    MarketingActivityStatus Status,
    MarketingActivityType Type,
    string? MarketingEffect
);

public record MarketingActivityStats
{
    public int LeadCount { get; set; }
    public int SaleCount { get; set; }
    public decimal? TotalBudget { get; set; }
    public decimal? TotalCost { get; set; }
    public decimal ExpectedRevenue { get; set; }
    public decimal ActualRevenue { get; set; }
}
