using AspireCRM.DataLayer.Repositories;
using AspireCRM.Domain.Sales;
using Microsoft.EntityFrameworkCore;

namespace AspireCRM.ApiService.Endpoints;

public static class SalesPlanEndpoints
{
    public static void MapSalesPlanEndpoints(this WebApplication app)
    {
        var api = app.MapGroup("/api/sales-plans");

        api.MapGet("/", async (IRepository<SalesPlan> repo) =>
            Results.Ok(await repo.GetAllAsync()));

        api.MapGet("/{id:long}", async (long id, IRepository<SalesPlan> repo) =>
        {
            var plan = await repo.GetByIdAsync(id);
            return plan is null ? Results.NotFound() : Results.Ok(plan);
        });

        api.MapGet("/by-year/{year:int}", async (int year, IRepository<SalesPlan> repo) =>
        {
            var plans = await repo.FindAsync(p => p.Year == year);
            return Results.Ok(plans);
        });

        api.MapPost("/", async (CreateSalesPlanRequest request, IRepository<SalesPlan> repo) =>
        {
            var plan = new SalesPlan
            {
                Year = request.Year,
                Month = request.Month,
                PlannedAmount = request.PlannedAmount,
                ActualAmount = request.ActualAmount,
                Description = request.Description?.Trim()
            };
            var created = await repo.AddAsync(plan);
            return Results.Created($"/api/sales-plans/{created.Id}", created);
        });

        api.MapPut("/{id:long}", async (long id, UpdateSalesPlanRequest request, IRepository<SalesPlan> repo) =>
        {
            var plan = await repo.GetByIdAsync(id);
            if (plan is null) return Results.NotFound();

            plan.Year = request.Year;
            plan.Month = request.Month;
            plan.PlannedAmount = request.PlannedAmount;
            plan.ActualAmount = request.ActualAmount;
            plan.Description = request.Description?.Trim();
            await repo.UpdateAsync(plan);
            return Results.NoContent();
        });

        api.MapDelete("/{id:long}", async (long id, IRepository<SalesPlan> repo) =>
        {
            var plan = await repo.GetByIdAsync(id);
            if (plan is null) return Results.NotFound();
            await repo.DeleteAsync(plan);
            return Results.NoContent();
        });
    }
}

public record CreateSalesPlanRequest(
    int Year,
    int Month,
    decimal PlannedAmount,
    decimal? ActualAmount,
    string? Description
);

public record UpdateSalesPlanRequest(
    int Year,
    int Month,
    decimal PlannedAmount,
    decimal? ActualAmount,
    string? Description
);