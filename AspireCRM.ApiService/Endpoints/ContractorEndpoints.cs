using AspireCRM.DataLayer;
using AspireCRM.DataLayer.Repositories;
using AspireCRM.Domain.Common;
using AspireCRM.Domain.Contractors;
using Microsoft.EntityFrameworkCore;

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

        api.MapGet("/{id:long}/details", async (long id, AspireCRMDbContext db, ITenantService tenantService) =>
        {
            if (!tenantService.TenantId.HasValue)
                return Results.Unauthorized();

            var contractor = await db.Contractors
                .Include(c => c.LegalAddress)
                .Include(c => c.PostalAddress)
                .Include(c => c.Emails)
                .Include(c => c.Phones)
                .Include(c => c.Contacts)
                .Include(c => c.Sales)
                .Include(c => c.Relationships)
                .FirstOrDefaultAsync(c => c.Id == id && c.TenantId == tenantService.TenantId.Value && !c.IsDeleted);

            return contractor is null ? Results.NotFound() : Results.Ok(contractor);
        });

        api.MapPost("/", async (Contractor contractor, IRepository<Contractor> repo) =>
        {
            var created = await repo.AddAsync(contractor);
            return Results.Created($"/api/contractors/{created.Id}", created);
        });

        api.MapPut("/{id:long}", async (long id, Contractor contractor, AspireCRMDbContext db, ITenantService tenantService) =>
        {
            if (id != contractor.Id) return Results.BadRequest();

            var existing = await db.Contractors
                .Include(c => c.LegalAddress)
                .Include(c => c.PostalAddress)
                .FirstOrDefaultAsync(c => c.Id == id && c.TenantId == tenantService.TenantId.Value && !c.IsDeleted);

            if (existing is null) return Results.NotFound();

            existing.Name = contractor.Name;
            existing.Description = contractor.Description;
            existing.Fax = contractor.Fax;
            existing.Site = contractor.Site;
            existing.INN = contractor.INN;
            existing.AnnualIncome = contractor.AnnualIncome;
            existing.CompanyDay = contractor.CompanyDay;
            existing.ResponsibleId = contractor.ResponsibleId;
            existing.RegionId = contractor.RegionId;
            existing.IndustryId = contractor.IndustryId;
            existing.ContractorTypeId = contractor.ContractorTypeId;

            if (contractor.LegalAddress is not null)
            {
                if (existing.LegalAddress is null)
                {
                    contractor.LegalAddress.TenantId = existing.TenantId;
                    existing.LegalAddress = contractor.LegalAddress;
                }
                else
                {
                    existing.LegalAddress.Country = contractor.LegalAddress.Country;
                    existing.LegalAddress.City = contractor.LegalAddress.City;
                    existing.LegalAddress.Street = contractor.LegalAddress.Street;
                    existing.LegalAddress.Building = contractor.LegalAddress.Building;
                    existing.LegalAddress.Apartment = contractor.LegalAddress.Apartment;
                    existing.LegalAddress.ZipCode = contractor.LegalAddress.ZipCode;
                    existing.LegalAddress.FullAddress = contractor.LegalAddress.FullAddress;
                }
            }

            if (contractor.PostalAddress is not null)
            {
                if (existing.PostalAddress is null)
                {
                    contractor.PostalAddress.TenantId = existing.TenantId;
                    existing.PostalAddress = contractor.PostalAddress;
                }
                else
                {
                    existing.PostalAddress.Country = contractor.PostalAddress.Country;
                    existing.PostalAddress.City = contractor.PostalAddress.City;
                    existing.PostalAddress.Street = contractor.PostalAddress.Street;
                    existing.PostalAddress.Building = contractor.PostalAddress.Building;
                    existing.PostalAddress.Apartment = contractor.PostalAddress.Apartment;
                    existing.PostalAddress.ZipCode = contractor.PostalAddress.ZipCode;
                    existing.PostalAddress.FullAddress = contractor.PostalAddress.FullAddress;
                }
            }

            existing.UpdatedAt = DateTime.UtcNow;
            await db.SaveChangesAsync();
            return Results.NoContent();
        });

        api.MapDelete("/{id:long}", async (long id, IRepository<Contractor> repo) =>
        {
            var contractor = await repo.GetByIdAsync(id);
            if (contractor is null) return Results.NotFound();
            await repo.DeleteAsync(contractor);
            return Results.NoContent();
        });

        api.MapPost("/{id:long}/emails", async (long id, AddEmailRequest request, AspireCRMDbContext db, ITenantService tenantService) =>
        {
            if (!tenantService.TenantId.HasValue)
                return Results.Unauthorized();

            var contractor = await db.Contractors
                .Include(c => c.Emails)
                .FirstOrDefaultAsync(c => c.Id == id && c.TenantId == tenantService.TenantId.Value && !c.IsDeleted);

            if (contractor is null) return Results.NotFound();

            var email = new Email
            {
                EmailAddress = request.EmailAddress,
                Description = request.Description,
                TenantId = tenantService.TenantId.Value
            };
            contractor.Emails.Add(email);
            await db.SaveChangesAsync();
            return Results.Created($"/api/contractors/{id}/emails/{email.Id}", email);
        });

        api.MapDelete("/{id:long}/emails/{emailId:long}", async (long id, long emailId, AspireCRMDbContext db, ITenantService tenantService) =>
        {
            if (!tenantService.TenantId.HasValue)
                return Results.Unauthorized();

            var contractor = await db.Contractors
                .Include(c => c.Emails)
                .FirstOrDefaultAsync(c => c.Id == id && c.TenantId == tenantService.TenantId.Value && !c.IsDeleted);

            if (contractor is null) return Results.NotFound();

            var email = contractor.Emails.FirstOrDefault(e => e.Id == emailId);
            if (email is null) return Results.NotFound();

            contractor.Emails.Remove(email);
            await db.SaveChangesAsync();
            return Results.NoContent();
        });

        api.MapPost("/{id:long}/phones", async (long id, AddPhoneRequest request, AspireCRMDbContext db, ITenantService tenantService) =>
        {
            if (!tenantService.TenantId.HasValue)
                return Results.Unauthorized();

            var contractor = await db.Contractors
                .Include(c => c.Phones)
                .FirstOrDefaultAsync(c => c.Id == id && c.TenantId == tenantService.TenantId.Value && !c.IsDeleted);

            if (contractor is null) return Results.NotFound();

            var phone = new Phone
            {
                PhoneNumber = request.PhoneNumber,
                Description = request.Description,
                TenantId = tenantService.TenantId.Value
            };
            contractor.Phones.Add(phone);
            await db.SaveChangesAsync();
            return Results.Created($"/api/contractors/{id}/phones/{phone.Id}", phone);
        });

        api.MapDelete("/{id:long}/phones/{phoneId:long}", async (long id, long phoneId, AspireCRMDbContext db, ITenantService tenantService) =>
        {
            if (!tenantService.TenantId.HasValue)
                return Results.Unauthorized();

            var contractor = await db.Contractors
                .Include(c => c.Phones)
                .FirstOrDefaultAsync(c => c.Id == id && c.TenantId == tenantService.TenantId.Value && !c.IsDeleted);

            if (contractor is null) return Results.NotFound();

            var phone = contractor.Phones.FirstOrDefault(p => p.Id == phoneId);
            if (phone is null) return Results.NotFound();

            contractor.Phones.Remove(phone);
            await db.SaveChangesAsync();
            return Results.NoContent();
        });
    }
}

public record AddEmailRequest(string EmailAddress, string? Description);
public record AddPhoneRequest(string PhoneNumber, string? Description);