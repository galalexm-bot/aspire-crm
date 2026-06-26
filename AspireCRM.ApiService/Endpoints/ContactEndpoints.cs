using AspireCRM.DataLayer.Repositories;
using AspireCRM.Domain.Contractors;

namespace AspireCRM.ApiService.Endpoints;

public static class ContactEndpoints
{
    public static void MapContactEndpoints(this WebApplication app)
    {
        var api = app.MapGroup("/api/contacts");

        api.MapGet("/", async (IRepository<Contact> repo, int page = 1, int pageSize = 20, long? contractorId = null) =>
        {
            var filter = contractorId.HasValue
                ? (System.Linq.Expressions.Expression<Func<Contact, bool>>)(c => c.ContractorId == contractorId.Value)
                : null;
            return Results.Ok(await repo.GetPagedAsync(page, pageSize, filter));
        });

        api.MapGet("/{id:long}", async (long id, IRepository<Contact> repo) =>
        {
            var contact = await repo.GetByIdAsync(id);
            return contact is null ? Results.NotFound() : Results.Ok(contact);
        });

        api.MapPost("/", async (Contact contact, IRepository<Contact> repo) =>
        {
            var created = await repo.AddAsync(contact);
            return Results.Created($"/api/contacts/{created.Id}", created);
        });

        api.MapPut("/{id:long}", async (long id, Contact contact, IRepository<Contact> repo) =>
        {
            if (id != contact.Id) return Results.BadRequest();
            await repo.UpdateAsync(contact);
            return Results.NoContent();
        });

        api.MapDelete("/{id:long}", async (long id, IRepository<Contact> repo) =>
        {
            var contact = await repo.GetByIdAsync(id);
            if (contact is null) return Results.NotFound();
            await repo.DeleteAsync(contact);
            return Results.NoContent();
        });
    }
}