using AspireCRM.DataLayer;
using AspireCRM.DataLayer.Repositories;
using AspireCRM.Domain.Sales;
using Microsoft.EntityFrameworkCore;

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

        api.MapPut("/{id:long}", async (long id, Sale sale, AspireCRMDbContext db, ITenantService tenantService) =>
        {
            if (id != sale.Id) return Results.BadRequest();

            if (!tenantService.TenantId.HasValue)
                return Results.Unauthorized();

            var existing = await db.Sales
                .FirstOrDefaultAsync(s => s.Id == id && s.TenantId == tenantService.TenantId.Value && !s.IsDeleted);

            if (existing is null) return Results.NotFound();

            existing.Name = sale.Name;
            existing.ShortStatus = sale.ShortStatus;
            existing.Description = sale.Description;
            existing.MarketingEffect = sale.MarketingEffect;
            existing.SalesVolume = sale.SalesVolume;
            existing.Priority = sale.Priority;
            existing.ContractorId = sale.ContractorId;
            existing.CurrencyId = sale.CurrencyId;
            existing.ResponsibleId = sale.ResponsibleId;
            existing.SaleTypeId = sale.SaleTypeId;
            existing.SaleStageId = sale.SaleStageId;
            existing.RegionId = sale.RegionId;
            existing.ContractorIndustryId = sale.ContractorIndustryId;
            existing.SaleFunnelId = sale.SaleFunnelId;
            existing.StartDate = sale.StartDate;
            existing.EndDate = sale.EndDate;
            existing.UpdatedAt = DateTime.UtcNow;

            await db.SaveChangesAsync();
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