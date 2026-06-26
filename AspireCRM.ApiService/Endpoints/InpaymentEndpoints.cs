using AspireCRM.DataLayer.Repositories;
using AspireCRM.Domain.Payments;

namespace AspireCRM.ApiService.Endpoints;

public static class InpaymentEndpoints
{
    public static void MapInpaymentEndpoints(this WebApplication app)
    {
        var api = app.MapGroup("/api/inpayments");

        api.MapGet("/", async (IRepository<Inpayment> repo, int page = 1, int pageSize = 20, long? saleId = null) =>
        {
            var filter = saleId.HasValue
                ? (System.Linq.Expressions.Expression<Func<Inpayment, bool>>)(i => i.SaleId == saleId.Value)
                : null;
            return Results.Ok(await repo.GetPagedAsync(page, pageSize, filter));
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
    }
}