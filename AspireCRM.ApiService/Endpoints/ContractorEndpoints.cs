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

        api.MapGet("/list", async (IRepository<Contractor> repo) =>
            Results.Ok(await repo.GetAllAsync()));

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

        api.MapPost("/", async (CreateContractorRequest request, AspireCRMDbContext db, ITenantService tenantService) =>
        {
            if (!tenantService.TenantId.HasValue)
                return Results.Unauthorized();

            Contractor contractor = request.SubType switch
            {
                "Legal" => new ContractorLegal
                {
                    Name = request.Name,
                    Description = request.Description,
                    Fax = request.Fax,
                    Site = request.Site,
                    INN = request.INN,
                    AnnualIncome = request.AnnualIncome,
                    CompanyDay = request.CompanyDay,
                    ResponsibleId = request.ResponsibleId,
                    RegionId = request.RegionId,
                    IndustryId = request.IndustryId,
                    ContractorTypeId = request.ContractorTypeId,
                    LegalFormId = request.LegalFormId,
                    Staff = request.Staff,
                    OGRN = request.OGRN,
                    KPP = request.KPP,
                },
                "Individual" => new ContractorIndividual
                {
                    Name = request.Name,
                    Description = request.Description,
                    Fax = request.Fax,
                    Site = request.Site,
                    INN = request.INN,
                    AnnualIncome = request.AnnualIncome,
                    CompanyDay = request.CompanyDay,
                    ResponsibleId = request.ResponsibleId,
                    RegionId = request.RegionId,
                    IndustryId = request.IndustryId,
                    ContractorTypeId = request.ContractorTypeId,
                    FirstName = request.FirstName ?? string.Empty,
                    SecondName = request.SecondName,
                    MiddleName = request.MiddleName,
                    DocumentTypeId = request.DocumentTypeId,
                    DocumentSeries = request.DocumentSeries,
                    DocumentNumber = request.DocumentNumber,
                    DocumentIssued = request.DocumentIssued,
                    DocumentIssueDate = request.DocumentIssueDate ?? DateTime.UtcNow,
                    DocumentEndDate = request.DocumentEndDate,
                },
                _ => new Contractor
                {
                    Name = request.Name,
                    Description = request.Description,
                    Fax = request.Fax,
                    Site = request.Site,
                    INN = request.INN,
                    AnnualIncome = request.AnnualIncome,
                    CompanyDay = request.CompanyDay,
                    ResponsibleId = request.ResponsibleId,
                    RegionId = request.RegionId,
                    IndustryId = request.IndustryId,
                    ContractorTypeId = request.ContractorTypeId,
                }
            };

            contractor.TenantId = tenantService.TenantId.Value;
            contractor.CreatedAt = DateTime.UtcNow;
            db.Contractors.Add(contractor);
            await db.SaveChangesAsync();
            return Results.Created($"/api/contractors/{contractor.Id}", contractor);
        });

        api.MapPut("/{id:long}", async (long id, UpdateContractorRequest request, AspireCRMDbContext db, ITenantService tenantService) =>
        {
            if (!tenantService.TenantId.HasValue)
                return Results.Unauthorized();

            var existing = await db.Contractors
                .Include(c => c.LegalAddress)
                .Include(c => c.PostalAddress)
                .FirstOrDefaultAsync(c => c.Id == id && c.TenantId == tenantService.TenantId.Value && !c.IsDeleted);

            if (existing is null) return Results.NotFound();

            existing.Name = request.Name;
            existing.Description = request.Description;
            existing.Fax = request.Fax;
            existing.Site = request.Site;
            existing.INN = request.INN;
            existing.AnnualIncome = request.AnnualIncome;
            existing.CompanyDay = request.CompanyDay;
            existing.ResponsibleId = request.ResponsibleId;
            existing.RegionId = request.RegionId;
            existing.IndustryId = request.IndustryId;
            existing.ContractorTypeId = request.ContractorTypeId;

            if (existing is ContractorLegal legal)
            {
                legal.LegalFormId = request.LegalFormId;
                legal.Staff = request.Staff;
                legal.OGRN = request.OGRN;
                legal.KPP = request.KPP;
            }
            else if (existing is ContractorIndividual individual)
            {
                individual.FirstName = request.FirstName ?? string.Empty;
                individual.SecondName = request.SecondName;
                individual.MiddleName = request.MiddleName;
                individual.DocumentTypeId = request.DocumentTypeId;
                individual.DocumentSeries = request.DocumentSeries;
                individual.DocumentNumber = request.DocumentNumber;
                individual.DocumentIssued = request.DocumentIssued;
                individual.DocumentIssueDate = request.DocumentIssueDate ?? DateTime.UtcNow;
                individual.DocumentEndDate = request.DocumentEndDate;
            }

            if (request.LegalAddress is not null)
            {
                if (existing.LegalAddress is null)
                {
                    existing.LegalAddress = new Address
                    {
                        Country = request.LegalAddress.Country,
                        City = request.LegalAddress.City,
                        Street = request.LegalAddress.Street,
                        Building = request.LegalAddress.Building,
                        Apartment = request.LegalAddress.Apartment,
                        ZipCode = request.LegalAddress.ZipCode,
                        FullAddress = request.LegalAddress.FullAddress,
                        TenantId = existing.TenantId
                    };
                }
                else
                {
                    existing.LegalAddress.Country = request.LegalAddress.Country;
                    existing.LegalAddress.City = request.LegalAddress.City;
                    existing.LegalAddress.Street = request.LegalAddress.Street;
                    existing.LegalAddress.Building = request.LegalAddress.Building;
                    existing.LegalAddress.Apartment = request.LegalAddress.Apartment;
                    existing.LegalAddress.ZipCode = request.LegalAddress.ZipCode;
                    existing.LegalAddress.FullAddress = request.LegalAddress.FullAddress;
                }
            }

            if (request.PostalAddress is not null)
            {
                if (existing.PostalAddress is null)
                {
                    existing.PostalAddress = new Address
                    {
                        Country = request.PostalAddress.Country,
                        City = request.PostalAddress.City,
                        Street = request.PostalAddress.Street,
                        Building = request.PostalAddress.Building,
                        Apartment = request.PostalAddress.Apartment,
                        ZipCode = request.PostalAddress.ZipCode,
                        FullAddress = request.PostalAddress.FullAddress,
                        TenantId = existing.TenantId
                    };
                }
                else
                {
                    existing.PostalAddress.Country = request.PostalAddress.Country;
                    existing.PostalAddress.City = request.PostalAddress.City;
                    existing.PostalAddress.Street = request.PostalAddress.Street;
                    existing.PostalAddress.Building = request.PostalAddress.Building;
                    existing.PostalAddress.Apartment = request.PostalAddress.Apartment;
                    existing.PostalAddress.ZipCode = request.PostalAddress.ZipCode;
                    existing.PostalAddress.FullAddress = request.PostalAddress.FullAddress;
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

        api.MapGet("/{id:long}/addresses/legal", async (long id, AspireCRMDbContext db, ITenantService tenantService) =>
        {
            if (!tenantService.TenantId.HasValue)
                return Results.Unauthorized();

            var contractor = await db.Contractors
                .Include(c => c.LegalAddress)
                .FirstOrDefaultAsync(c => c.Id == id && c.TenantId == tenantService.TenantId.Value && !c.IsDeleted);

            if (contractor is null) return Results.NotFound();
            return contractor.LegalAddress is null ? Results.NotFound() : Results.Ok(contractor.LegalAddress);
        });

        api.MapPut("/{id:long}/addresses/legal", async (long id, AddressDto request, AspireCRMDbContext db, ITenantService tenantService) =>
        {
            if (!tenantService.TenantId.HasValue)
                return Results.Unauthorized();

            var contractor = await db.Contractors
                .Include(c => c.LegalAddress)
                .FirstOrDefaultAsync(c => c.Id == id && c.TenantId == tenantService.TenantId.Value && !c.IsDeleted);

            if (contractor is null) return Results.NotFound();

            if (contractor.LegalAddress is null)
            {
                contractor.LegalAddress = new Address
                {
                    Country = request.Country,
                    City = request.City,
                    Street = request.Street,
                    Building = request.Building,
                    Apartment = request.Apartment,
                    ZipCode = request.ZipCode,
                    FullAddress = request.FullAddress,
                    TenantId = tenantService.TenantId.Value
                };
            }
            else
            {
                contractor.LegalAddress.Country = request.Country;
                contractor.LegalAddress.City = request.City;
                contractor.LegalAddress.Street = request.Street;
                contractor.LegalAddress.Building = request.Building;
                contractor.LegalAddress.Apartment = request.Apartment;
                contractor.LegalAddress.ZipCode = request.ZipCode;
                contractor.LegalAddress.FullAddress = request.FullAddress;
            }

            contractor.UpdatedAt = DateTime.UtcNow;
            await db.SaveChangesAsync();
            return Results.NoContent();
        });

        api.MapGet("/{id:long}/addresses/postal", async (long id, AspireCRMDbContext db, ITenantService tenantService) =>
        {
            if (!tenantService.TenantId.HasValue)
                return Results.Unauthorized();

            var contractor = await db.Contractors
                .Include(c => c.PostalAddress)
                .FirstOrDefaultAsync(c => c.Id == id && c.TenantId == tenantService.TenantId.Value && !c.IsDeleted);

            if (contractor is null) return Results.NotFound();
            return contractor.PostalAddress is null ? Results.NotFound() : Results.Ok(contractor.PostalAddress);
        });

        api.MapPut("/{id:long}/addresses/postal", async (long id, AddressDto request, AspireCRMDbContext db, ITenantService tenantService) =>
        {
            if (!tenantService.TenantId.HasValue)
                return Results.Unauthorized();

            var contractor = await db.Contractors
                .Include(c => c.PostalAddress)
                .FirstOrDefaultAsync(c => c.Id == id && c.TenantId == tenantService.TenantId.Value && !c.IsDeleted);

            if (contractor is null) return Results.NotFound();

            if (contractor.PostalAddress is null)
            {
                contractor.PostalAddress = new Address
                {
                    Country = request.Country,
                    City = request.City,
                    Street = request.Street,
                    Building = request.Building,
                    Apartment = request.Apartment,
                    ZipCode = request.ZipCode,
                    FullAddress = request.FullAddress,
                    TenantId = tenantService.TenantId.Value
                };
            }
            else
            {
                contractor.PostalAddress.Country = request.Country;
                contractor.PostalAddress.City = request.City;
                contractor.PostalAddress.Street = request.Street;
                contractor.PostalAddress.Building = request.Building;
                contractor.PostalAddress.Apartment = request.Apartment;
                contractor.PostalAddress.ZipCode = request.ZipCode;
                contractor.PostalAddress.FullAddress = request.FullAddress;
            }

            contractor.UpdatedAt = DateTime.UtcNow;
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

public record CreateContractorRequest(
    string Name,
    string SubType,
    string? Description,
    string? Fax,
    string? Site,
    string? INN,
    double? AnnualIncome,
    DateTime? CompanyDay,
    long? ResponsibleId,
    long? RegionId,
    long? IndustryId,
    long? ContractorTypeId,
    long? LegalFormId,
    long? Staff,
    string? OGRN,
    string? KPP,
    string? FirstName,
    string? SecondName,
    string? MiddleName,
    long? DocumentTypeId,
    string? DocumentSeries,
    string? DocumentNumber,
    string? DocumentIssued,
    DateTime? DocumentIssueDate,
    DateTime? DocumentEndDate
);

public record UpdateContractorRequest(
    string Name,
    string? Description,
    string? Fax,
    string? Site,
    string? INN,
    double? AnnualIncome,
    DateTime? CompanyDay,
    long? ResponsibleId,
    long? RegionId,
    long? IndustryId,
    long? ContractorTypeId,
    long? LegalFormId,
    long? Staff,
    string? OGRN,
    string? KPP,
    string? FirstName,
    string? SecondName,
    string? MiddleName,
    long? DocumentTypeId,
    string? DocumentSeries,
    string? DocumentNumber,
    string? DocumentIssued,
    DateTime? DocumentIssueDate,
    DateTime? DocumentEndDate,
    AddressDto? LegalAddress,
    AddressDto? PostalAddress
);

public record AddressDto(
    string? Country,
    string? City,
    string? Street,
    string? Building,
    string? Apartment,
    string? ZipCode,
    string? FullAddress
);