using AspireCRM.DataLayer;
using AspireCRM.DataLayer.Repositories;
using AspireCRM.Domain.Payments;
using Microsoft.EntityFrameworkCore;

namespace AspireCRM.ApiService.Endpoints;

public static class InpaymentEndpoints
{
    public static void MapInpaymentEndpoints(this WebApplication app)
    {
        var api = app.MapGroup("/api/inpayments");

        api.MapGet("/list", async (AspireCRMDbContext db, ITenantService tenantService, long? saleId = null) =>
        {
            if (!tenantService.TenantId.HasValue)
                return Results.Unauthorized();

            var query = db.Inpayments
                .Include(i => i.Sale)
                .Include(i => i.Contractor)
                .Where(i => i.TenantId == tenantService.TenantId.Value && !i.IsDeleted);

            if (saleId.HasValue)
                query = query.Where(i => i.SaleId == saleId.Value);

            var items = await query
                .OrderByDescending(i => i.CreationDate)
                .Select(i => new InpaymentListItem
                {
                    Id = i.Id,
                    Name = i.Name,
                    Sum = i.Sum,
                    Status = i.Status,
                    Date = i.Date,
                    CreationDate = i.CreationDate,
                    SaleId = i.SaleId,
                    SaleName = i.Sale.Name,
                    ContractorId = i.ContractorId,
                    ContractorName = i.Contractor.Name
                })
                .ToListAsync();

            return Results.Ok(items);
        });

        api.MapGet("/{id:long}", async (long id, IRepository<Inpayment> repo) =>
        {
            var inpayment = await repo.GetByIdAsync(id);
            return inpayment is null ? Results.NotFound() : Results.Ok(inpayment);
        });

        api.MapPost("/", async (Inpayment inpayment, IRepository<Inpayment> repo) =>
        {
            var created = await repo.AddAsync(inpayment);
            return Results.Created($"/api/inpayments/{created.Id}", created);
        });

        api.MapPut("/{id:long}", async (long id, Inpayment inpayment, IRepository<Inpayment> repo) =>
        {
            if (id != inpayment.Id) return Results.BadRequest();
            await repo.UpdateAsync(inpayment);
            return Results.NoContent();
        });

        api.MapDelete("/{id:long}", async (long id, IRepository<Inpayment> repo) =>
        {
            var inpayment = await repo.GetByIdAsync(id);
            if (inpayment is null) return Results.NotFound();
            await repo.DeleteAsync(inpayment);
            return Results.NoContent();
        });

        api.MapPost("/{id:long}/change-status", async (long id, InpaymentStatusChangeRequest request, AspireCRMDbContext db, ITenantService tenantService) =>
        {
            if (!tenantService.TenantId.HasValue)
                return Results.Unauthorized();

            var inpayment = await db.Inpayments
                .FirstOrDefaultAsync(i => i.Id == id && i.TenantId == tenantService.TenantId.Value && !i.IsDeleted);

            if (inpayment is null) return Results.NotFound();

            if (!Enum.TryParse<InpaymentStatus>(request.Status, out var newStatus))
                return Results.BadRequest("Некорректный статус");

            inpayment.Status = newStatus;
            inpayment.ChangeStatusDate = DateTime.UtcNow;
            inpayment.ChangeStatusComment = request.Comment;
            inpayment.ChangeDate = DateTime.UtcNow;

            await db.SaveChangesAsync();
            return Results.NoContent();
        });

        api.MapGet("/summary", async (AspireCRMDbContext db, ITenantService tenantService) =>
        {
            if (!tenantService.TenantId.HasValue)
                return Results.Unauthorized();

            var now = DateTime.UtcNow;

            var query = db.Inpayments
                .Where(i => i.TenantId == tenantService.TenantId.Value && !i.IsDeleted);

            var planSum = await query.Where(i => i.Status == InpaymentStatus.InPlan).SumAsync(i => (double?)i.Sum) ?? 0;
            var receivedSum = await query.Where(i => i.Status == InpaymentStatus.Received).SumAsync(i => (double?)i.Sum) ?? 0;
            var cancelledSum = await query.Where(i => i.Status == InpaymentStatus.Cancelled).SumAsync(i => (double?)i.Sum) ?? 0;
            var overdueCount = await query.CountAsync(i => i.Status == InpaymentStatus.InPlan && i.Date < now);
            var totalCount = await query.CountAsync();
            var planCount = await query.CountAsync(i => i.Status == InpaymentStatus.InPlan);
            var receivedCount = await query.CountAsync(i => i.Status == InpaymentStatus.Received);

            return Results.Ok(new InpaymentSummary
            {
                PlanSum = planSum,
                ReceivedSum = receivedSum,
                CancelledSum = cancelledSum,
                OverdueCount = overdueCount,
                TotalCount = totalCount,
                PlanCount = planCount,
                ReceivedCount = receivedCount
            });
        });
    }
}

public class InpaymentListItem
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Sum { get; set; }
    public InpaymentStatus Status { get; set; }
    public DateTime? Date { get; set; }
    public DateTime CreationDate { get; set; }
    public long SaleId { get; set; }
    public string SaleName { get; set; } = string.Empty;
    public long ContractorId { get; set; }
    public string ContractorName { get; set; } = string.Empty;
}

public record InpaymentStatusChangeRequest(string Status, string? Comment);