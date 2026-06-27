using AspireCRM.DataLayer;
using AspireCRM.DataLayer.Repositories;
using AspireCRM.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace AspireCRM.ApiService.Endpoints;

public static class SettingsEndpoints
{
    public static void MapSettingsEndpoints(this WebApplication app)
    {
        var api = app.MapGroup("/api/settings");

        api.MapGet("/", async (AspireCRMDbContext db, ITenantService tenantService) =>
        {
            var tenantId = tenantService.TenantId;
            if (tenantId is null) return Results.Unauthorized();

            var settings = await db.Set<CrmSettings>()
                .Include(s => s.DefaultCurrency)
                .FirstOrDefaultAsync(s => s.TenantId == tenantId.Value && !s.IsDeleted);

            if (settings is null)
            {
                settings = new CrmSettings
                {
                    TenantId = tenantId.Value,
                    CreatedAt = DateTime.UtcNow,
                    CreatedById = 0
                };
                db.Add(settings);
                await db.SaveChangesAsync();
            }

            return Results.Ok(new
            {
                settings.Id,
                settings.DefaultCurrencyId,
                DefaultCurrencyName = settings.DefaultCurrency?.Name,
                settings.DefaultCountry,
                settings.CompanyName,
                settings.CompanyAddress,
                settings.CompanyPhone,
                settings.CompanyEmail,
            });
        });

        api.MapPut("/", async (AspireCRMDbContext db, ITenantService tenantService, SettingsUpdateRequest request) =>
        {
            var tenantId = tenantService.TenantId;
            if (tenantId is null) return Results.Unauthorized();

            var settings = await db.Set<CrmSettings>()
                .FirstOrDefaultAsync(s => s.TenantId == tenantId.Value && !s.IsDeleted);

            if (settings is null)
            {
                settings = new CrmSettings
                {
                    TenantId = tenantId.Value,
                    CreatedAt = DateTime.UtcNow,
                    CreatedById = 0
                };
                db.Add(settings);
            }

            settings.DefaultCurrencyId = request.DefaultCurrencyId;
            settings.DefaultCountry = request.DefaultCountry;
            settings.CompanyName = request.CompanyName;
            settings.CompanyAddress = request.CompanyAddress;
            settings.CompanyPhone = request.CompanyPhone;
            settings.CompanyEmail = request.CompanyEmail;
            settings.UpdatedAt = DateTime.UtcNow;

            await db.SaveChangesAsync();
            return Results.Ok();
        });
    }

    private sealed record SettingsUpdateRequest
    {
        public long? DefaultCurrencyId { get; init; }
        public string? DefaultCountry { get; init; }
        public string? CompanyName { get; init; }
        public string? CompanyAddress { get; init; }
        public string? CompanyPhone { get; init; }
        public string? CompanyEmail { get; init; }
    }
}