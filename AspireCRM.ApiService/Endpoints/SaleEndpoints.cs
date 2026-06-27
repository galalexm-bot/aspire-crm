using AspireCRM.DataLayer;
using AspireCRM.DataLayer.Repositories;
using AspireCRM.Domain.Common;
using AspireCRM.Domain.Products;
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

        api.MapGet("/{saleId:long}/products", async (long saleId, AspireCRMDbContext db, ITenantService tenantService) =>
        {
            if (!tenantService.TenantId.HasValue)
                return Results.Unauthorized();

            var products = await db.SaleProducts
                .Include(sp => sp.Product)
                .Where(sp => sp.SaleId == saleId && sp.TenantId == tenantService.TenantId.Value && !sp.IsDeleted)
                .ToListAsync();

            return Results.Ok(products);
        });

        api.MapPost("/{saleId:long}/products", async (long saleId, SaleProductRequest request, AspireCRMDbContext db, ITenantService tenantService) =>
        {
            if (!tenantService.TenantId.HasValue)
                return Results.Unauthorized();

            var sale = await db.Sales
                .FirstOrDefaultAsync(s => s.Id == saleId && s.TenantId == tenantService.TenantId.Value && !s.IsDeleted);

            if (sale is null) return Results.NotFound();

            var product = await db.Products
                .FirstOrDefaultAsync(p => p.Id == request.ProductId && p.TenantId == tenantService.TenantId.Value && !p.IsDeleted);

            if (product is null) return Results.NotFound();

            var saleProduct = new SaleProduct
            {
                SaleId = saleId,
                ProductId = request.ProductId,
                Quantity = request.Quantity,
                Price = request.Price,
                Discount = request.Discount,
                TenantId = tenantService.TenantId.Value
            };

            db.SaleProducts.Add(saleProduct);
            await db.SaveChangesAsync();
            return Results.Created($"/api/sales/{saleId}/products/{saleProduct.Id}", saleProduct);
        });

        api.MapPut("/{saleId:long}/products/{productId:long}", async (long saleId, long productId, SaleProductRequest request, AspireCRMDbContext db, ITenantService tenantService) =>
        {
            if (!tenantService.TenantId.HasValue)
                return Results.Unauthorized();

            var saleProduct = await db.SaleProducts
                .FirstOrDefaultAsync(sp => sp.Id == productId && sp.SaleId == saleId && sp.TenantId == tenantService.TenantId.Value && !sp.IsDeleted);

            if (saleProduct is null) return Results.NotFound();

            saleProduct.ProductId = request.ProductId;
            saleProduct.Quantity = request.Quantity;
            saleProduct.Price = request.Price;
            saleProduct.Discount = request.Discount;
            saleProduct.UpdatedAt = DateTime.UtcNow;

            await db.SaveChangesAsync();
            return Results.NoContent();
        });

        api.MapDelete("/{saleId:long}/products/{productId:long}", async (long saleId, long productId, AspireCRMDbContext db, ITenantService tenantService) =>
        {
            if (!tenantService.TenantId.HasValue)
                return Results.Unauthorized();

            var saleProduct = await db.SaleProducts
                .FirstOrDefaultAsync(sp => sp.Id == productId && sp.SaleId == saleId && sp.TenantId == tenantService.TenantId.Value && !sp.IsDeleted);

            if (saleProduct is null) return Results.NotFound();

            saleProduct.IsDeleted = true;
            await db.SaveChangesAsync();
            return Results.NoContent();
        });

        api.MapPost("/{id:long}/change-stage", async (long id, SaleStageChangeRequest request, HttpContext http,
            AspireCRMDbContext db, ITenantService tenantService, IRepository<Comment> commentRepo, AuditService auditService) =>
        {
            if (!tenantService.TenantId.HasValue)
                return Results.Unauthorized();

            var sale = await db.Sales
                .FirstOrDefaultAsync(s => s.Id == id && s.TenantId == tenantService.TenantId.Value && !s.IsDeleted);

            if (sale is null) return Results.NotFound();

            var stage = await db.SaleStages
                .FirstOrDefaultAsync(s => s.Id == request.StageId && s.TenantId == tenantService.TenantId.Value && !s.IsDeleted);

            if (stage is null) return Results.NotFound("Этап не найден");

            var oldStage = sale.SaleStageId.HasValue
                ? await db.SaleStages.FirstOrDefaultAsync(s => s.Id == sale.SaleStageId.Value)
                : null;

            sale.PreviousStageId = sale.SaleStageId;
            sale.SaleStageId = request.StageId;
            sale.StageChangeDate = DateTime.UtcNow;
            sale.UpdatedAt = DateTime.UtcNow;

            if (!string.IsNullOrWhiteSpace(request.Comment))
            {
                var comment = new Comment
                {
                    Text = request.Comment,
                    AuthorId = GetUserId(http),
                    CreationDate = DateTime.UtcNow,
                    TenantId = tenantService.TenantId.Value
                };
                await commentRepo.AddAsync(comment);
                sale.StageChangeCommentId = comment.Id;
            }

            await db.SaveChangesAsync();

            await auditService.LogAsync("Sale", id, "SaleStage", oldStage?.Name, stage.Name, GetUserId(http), request.Comment);

            return Results.Ok(sale);
        });

        api.MapPost("/{id:long}/change-status", async (long id, SaleStatusChangeRequest request, HttpContext http,
            AspireCRMDbContext db, ITenantService tenantService, IRepository<Comment> commentRepo, AuditService auditService) =>
        {
            if (!tenantService.TenantId.HasValue)
                return Results.Unauthorized();

            var sale = await db.Sales
                .FirstOrDefaultAsync(s => s.Id == id && s.TenantId == tenantService.TenantId.Value && !s.IsDeleted);

            if (sale is null) return Results.NotFound();

            if (!Enum.TryParse<SaleStatus>(request.Status, out var newStatus))
                return Results.BadRequest("Некорректный статус");

            var oldStatus = sale.SaleStatus.ToString();
            sale.SaleStatus = newStatus;
            sale.StatusChangeDate = DateTime.UtcNow;
            sale.UpdatedAt = DateTime.UtcNow;

            if (!string.IsNullOrWhiteSpace(request.Comment))
            {
                var comment = new Comment
                {
                    Text = request.Comment,
                    AuthorId = GetUserId(http),
                    CreationDate = DateTime.UtcNow,
                    TenantId = tenantService.TenantId.Value
                };
                await commentRepo.AddAsync(comment);
                sale.StatusChangeCommentId = comment.Id;
            }

            await db.SaveChangesAsync();

            await auditService.LogAsync("Sale", id, "SaleStatus", oldStatus, newStatus.ToString(), GetUserId(http), request.Comment);

            return Results.Ok(sale);
        });

        api.MapGet("/summary", async (AspireCRMDbContext db, ITenantService tenantService) =>
        {
            if (!tenantService.TenantId.HasValue)
                return Results.Unauthorized();

            var sales = db.Sales
                .Where(s => s.TenantId == tenantService.TenantId.Value && !s.IsDeleted);

            var now = DateTime.UtcNow;

            var summary = new SaleSummary
            {
                TotalActive = await sales.CountAsync(s => s.SaleStatus == SaleStatus.Active),
                TotalPostponed = await sales.CountAsync(s => s.SaleStatus == SaleStatus.Postponed),
                TotalPositiveClosed = await sales.CountAsync(s => s.SaleStatus == SaleStatus.PositiveClosed),
                TotalNegativeClosed = await sales.CountAsync(s => s.SaleStatus == SaleStatus.NegativeClosed),
                TotalSalesVolume = await sales.SumAsync(s => s.SalesVolume ?? 0),
                ActiveSalesVolume = await sales.Where(s => s.SaleStatus == SaleStatus.Active).SumAsync(s => s.SalesVolume ?? 0),
                OverdueCount = await sales.CountAsync(s => s.SaleStatus == SaleStatus.Active && s.EndDate != null && s.EndDate < now)
            };

            return Results.Ok(summary);
        });
    }

    private static long GetUserId(HttpContext http)
    {
        var claim = http.User.FindFirst("userId")?.Value;
        return long.TryParse(claim, out var userId) ? userId : 0;
    }
}

public record SaleProductRequest(long ProductId, double Quantity, double Price, double? Discount);
public record SaleStageChangeRequest(long StageId, string? Comment);
public record SaleStatusChangeRequest(string Status, string? Comment);
public record SaleSummary
{
    public int TotalActive { get; init; }
    public int TotalPostponed { get; init; }
    public int TotalPositiveClosed { get; init; }
    public int TotalNegativeClosed { get; init; }
    public double TotalSalesVolume { get; init; }
    public double ActiveSalesVolume { get; init; }
    public int OverdueCount { get; init; }
}