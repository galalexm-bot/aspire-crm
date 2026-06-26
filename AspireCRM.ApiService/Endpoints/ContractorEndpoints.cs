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
                .Include(c => c.BankAccounts)
                .Include(c => c.PaymentCards)
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

        api.MapGet("/{id:long}/bank-accounts", async (long id, AspireCRMDbContext db, ITenantService tenantService) =>
        {
            if (!tenantService.TenantId.HasValue)
                return Results.Unauthorized();

            var accounts = await db.BankAccounts
                .Where(ba => ba.ContractorId == id && ba.TenantId == tenantService.TenantId.Value && !ba.IsDeleted)
                .ToListAsync();

            return Results.Ok(accounts);
        });

        api.MapPost("/{id:long}/bank-accounts", async (long id, BankAccountRequest request, AspireCRMDbContext db, ITenantService tenantService) =>
        {
            if (!tenantService.TenantId.HasValue)
                return Results.Unauthorized();

            var contractor = await db.Contractors
                .FirstOrDefaultAsync(c => c.Id == id && c.TenantId == tenantService.TenantId.Value && !c.IsDeleted);

            if (contractor is null) return Results.NotFound();

            var account = new BankAccount
            {
                Number = request.Number,
                BIK = request.BIK,
                BankName = request.BankName,
                CorrespondentAccount = request.CorrespondentAccount,
                Description = request.Description,
                ContractorId = id,
                TenantId = tenantService.TenantId.Value
            };
            db.BankAccounts.Add(account);
            await db.SaveChangesAsync();
            return Results.Created($"/api/contractors/{id}/bank-accounts/{account.Id}", account);
        });

        api.MapPut("/{id:long}/bank-accounts/{accountId:long}", async (long id, long accountId, BankAccountRequest request, AspireCRMDbContext db, ITenantService tenantService) =>
        {
            if (!tenantService.TenantId.HasValue)
                return Results.Unauthorized();

            var account = await db.BankAccounts
                .FirstOrDefaultAsync(ba => ba.Id == accountId && ba.ContractorId == id && ba.TenantId == tenantService.TenantId.Value && !ba.IsDeleted);

            if (account is null) return Results.NotFound();

            account.Number = request.Number;
            account.BIK = request.BIK;
            account.BankName = request.BankName;
            account.CorrespondentAccount = request.CorrespondentAccount;
            account.Description = request.Description;
            await db.SaveChangesAsync();
            return Results.NoContent();
        });

        api.MapDelete("/{id:long}/bank-accounts/{accountId:long}", async (long id, long accountId, AspireCRMDbContext db, ITenantService tenantService) =>
        {
            if (!tenantService.TenantId.HasValue)
                return Results.Unauthorized();

            var account = await db.BankAccounts
                .FirstOrDefaultAsync(ba => ba.Id == accountId && ba.ContractorId == id && ba.TenantId == tenantService.TenantId.Value && !ba.IsDeleted);

            if (account is null) return Results.NotFound();

            account.IsDeleted = true;
            await db.SaveChangesAsync();
            return Results.NoContent();
        });

        api.MapGet("/{id:long}/payment-cards", async (long id, AspireCRMDbContext db, ITenantService tenantService) =>
        {
            if (!tenantService.TenantId.HasValue)
                return Results.Unauthorized();

            var cards = await db.PaymentCards
                .Where(pc => pc.ContractorId == id && pc.TenantId == tenantService.TenantId.Value && !pc.IsDeleted)
                .ToListAsync();

            return Results.Ok(cards);
        });

        api.MapPost("/{id:long}/payment-cards", async (long id, PaymentCardRequest request, AspireCRMDbContext db, ITenantService tenantService) =>
        {
            if (!tenantService.TenantId.HasValue)
                return Results.Unauthorized();

            var contractor = await db.Contractors
                .FirstOrDefaultAsync(c => c.Id == id && c.TenantId == tenantService.TenantId.Value && !c.IsDeleted);

            if (contractor is null) return Results.NotFound();

            var card = new PaymentCard
            {
                Number = request.Number,
                CardholderName = request.CardholderName,
                Description = request.Description,
                ContractorId = id,
                TenantId = tenantService.TenantId.Value
            };
            db.PaymentCards.Add(card);
            await db.SaveChangesAsync();
            return Results.Created($"/api/contractors/{id}/payment-cards/{card.Id}", card);
        });

        api.MapPut("/{id:long}/payment-cards/{cardId:long}", async (long id, long cardId, PaymentCardRequest request, AspireCRMDbContext db, ITenantService tenantService) =>
        {
            if (!tenantService.TenantId.HasValue)
                return Results.Unauthorized();

            var card = await db.PaymentCards
                .FirstOrDefaultAsync(pc => pc.Id == cardId && pc.ContractorId == id && pc.TenantId == tenantService.TenantId.Value && !pc.IsDeleted);

            if (card is null) return Results.NotFound();

            card.Number = request.Number;
            card.CardholderName = request.CardholderName;
            card.Description = request.Description;
            await db.SaveChangesAsync();
            return Results.NoContent();
        });

        api.MapDelete("/{id:long}/payment-cards/{cardId:long}", async (long id, long cardId, AspireCRMDbContext db, ITenantService tenantService) =>
        {
            if (!tenantService.TenantId.HasValue)
                return Results.Unauthorized();

            var card = await db.PaymentCards
                .FirstOrDefaultAsync(pc => pc.Id == cardId && pc.ContractorId == id && pc.TenantId == tenantService.TenantId.Value && !pc.IsDeleted);

            if (card is null) return Results.NotFound();

            card.IsDeleted = true;
            await db.SaveChangesAsync();
            return Results.NoContent();
        });
    }
}

public record AddEmailRequest(string EmailAddress, string? Description);
public record AddPhoneRequest(string PhoneNumber, string? Description);
public record BankAccountRequest(string Number, string? BIK, string? BankName, string? CorrespondentAccount, string? Description);
public record PaymentCardRequest(string Number, string? CardholderName, string? Description);