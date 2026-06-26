using AspireCRM.Domain.Common;
using AspireCRM.Domain.Contractors;
using AspireCRM.Domain.Leads;
using AspireCRM.Domain.Sales;
using Microsoft.EntityFrameworkCore;

namespace AspireCRM.DataLayer.Data;

public static class SeedData
{
    public static async Task InitializeAsync(AspireCRMDbContext context)
    {
        var defaultTenant = await SeedTenant(context);
        await SeedLookups(context, defaultTenant.Id);
        await context.SaveChangesAsync();
    }

    private static async Task<Tenant> SeedTenant(AspireCRMDbContext context)
    {
        var tenant = await context.Tenants.FirstOrDefaultAsync(t => t.Code == "default");
        if (tenant is not null) return tenant;

        tenant = new Tenant { Name = "Default Tenant", Code = "default" };
        context.Tenants.Add(tenant);
        return tenant;
    }

    private static async Task SeedLookups(AspireCRMDbContext context, long tId)
    {
        if (await context.LeadSources.AnyAsync()) return;
        var now = DateTime.UtcNow;

        context.LeadSources.AddRange(
            new LeadSource { Name = "Входящий звонок", TenantId = tId, CreatedAt = now, CreatedById = 1 },
            new LeadSource { Name = "Исходящий звонок", TenantId = tId, CreatedAt = now, CreatedById = 1 },
            new LeadSource { Name = "Веб-сайт", TenantId = tId, CreatedAt = now, CreatedById = 1 },
            new LeadSource { Name = "Рекомендация", TenantId = tId, CreatedAt = now, CreatedById = 1 },
            new LeadSource { Name = "Социальные сети", TenantId = tId, CreatedAt = now, CreatedById = 1 },
            new LeadSource { Name = "Email-рассылка", TenantId = tId, CreatedAt = now, CreatedById = 1 },
            new LeadSource { Name = "Конференция", TenantId = tId, CreatedAt = now, CreatedById = 1 },
            new LeadSource { Name = "Партнёр", TenantId = tId, CreatedAt = now, CreatedById = 1 }
        );

        context.LeadTypes.AddRange(
            new LeadType { Name = "Холодный", TenantId = tId, CreatedAt = now, CreatedById = 1 },
            new LeadType { Name = "Тёплый", TenantId = tId, CreatedAt = now, CreatedById = 1 },
            new LeadType { Name = "Горячий", TenantId = tId, CreatedAt = now, CreatedById = 1 }
        );

        context.ContractorRegions.AddRange(
            new ContractorRegion { Name = "Москва и Московская область", TenantId = tId, CreatedAt = now, CreatedById = 1 },
            new ContractorRegion { Name = "Санкт-Петербург и ЛО", TenantId = tId, CreatedAt = now, CreatedById = 1 },
            new ContractorRegion { Name = "Центральный федеральный округ", TenantId = tId, CreatedAt = now, CreatedById = 1 },
            new ContractorRegion { Name = "Северо-Западный федеральный округ", TenantId = tId, CreatedAt = now, CreatedById = 1 },
            new ContractorRegion { Name = "Южный федеральный округ", TenantId = tId, CreatedAt = now, CreatedById = 1 },
            new ContractorRegion { Name = "Приволжский федеральный округ", TenantId = tId, CreatedAt = now, CreatedById = 1 },
            new ContractorRegion { Name = "Уральский федеральный округ", TenantId = tId, CreatedAt = now, CreatedById = 1 },
            new ContractorRegion { Name = "Сибирский федеральный округ", TenantId = tId, CreatedAt = now, CreatedById = 1 },
            new ContractorRegion { Name = "Дальневосточный федеральный округ", TenantId = tId, CreatedAt = now, CreatedById = 1 }
        );

        context.ContractorIndustries.AddRange(
            new ContractorIndustry { Name = "Информационные технологии", TenantId = tId, CreatedAt = now, CreatedById = 1 },
            new ContractorIndustry { Name = "Финансы и страхование", TenantId = tId, CreatedAt = now, CreatedById = 1 },
            new ContractorIndustry { Name = "Розничная торговля", TenantId = tId, CreatedAt = now, CreatedById = 1 },
            new ContractorIndustry { Name = "Производство", TenantId = tId, CreatedAt = now, CreatedById = 1 },
            new ContractorIndustry { Name = "Строительство", TenantId = tId, CreatedAt = now, CreatedById = 1 },
            new ContractorIndustry { Name = "Медицина и фармацевтика", TenantId = tId, CreatedAt = now, CreatedById = 1 },
            new ContractorIndustry { Name = "Образование", TenantId = tId, CreatedAt = now, CreatedById = 1 },
            new ContractorIndustry { Name = "Транспорт и логистика", TenantId = tId, CreatedAt = now, CreatedById = 1 },
            new ContractorIndustry { Name = "Услуги", TenantId = tId, CreatedAt = now, CreatedById = 1 }
        );

        context.ContractorTypes.AddRange(
            new ContractorType { Name = "Клиент", TenantId = tId, CreatedAt = now, CreatedById = 1 },
            new ContractorType { Name = "Партнёр", TenantId = tId, CreatedAt = now, CreatedById = 1 },
            new ContractorType { Name = "Поставщик", TenantId = tId, CreatedAt = now, CreatedById = 1 },
            new ContractorType { Name = "Подрядчик", TenantId = tId, CreatedAt = now, CreatedById = 1 },
            new ContractorType { Name = "Конкурент", TenantId = tId, CreatedAt = now, CreatedById = 1 }
        );

        context.LegalForms.AddRange(
            new LegalForm { Name = "ООО", TenantId = tId, CreatedAt = now, CreatedById = 1 },
            new LegalForm { Name = "АО", TenantId = tId, CreatedAt = now, CreatedById = 1 },
            new LegalForm { Name = "ПАО", TenantId = tId, CreatedAt = now, CreatedById = 1 },
            new LegalForm { Name = "ИП", TenantId = tId, CreatedAt = now, CreatedById = 1 },
            new LegalForm { Name = "Самозанятый", TenantId = tId, CreatedAt = now, CreatedById = 1 }
        );

        context.Currencies.AddRange(
            new Currency { Name = "Российский рубль", Code = "RUB", TenantId = tId, CreatedAt = now, CreatedById = 1 },
            new Currency { Name = "Доллар США", Code = "USD", TenantId = tId, CreatedAt = now, CreatedById = 1 },
            new Currency { Name = "Евро", Code = "EUR", TenantId = tId, CreatedAt = now, CreatedById = 1 }
        );

        context.SaleTypes.AddRange(
            new SaleType { Name = "Продажа продукта", TenantId = tId, CreatedAt = now, CreatedById = 1 },
            new SaleType { Name = "Услуга", TenantId = tId, CreatedAt = now, CreatedById = 1 },
            new SaleType { Name = "Подписка", TenantId = tId, CreatedAt = now, CreatedById = 1 },
            new SaleType { Name = "Лицензия", TenantId = tId, CreatedAt = now, CreatedById = 1 }
        );

        context.SaleStages.AddRange(
            new SaleStage { Name = "Первичный контакт", SortOrder = 1, TenantId = tId, CreatedAt = now, CreatedById = 1 },
            new SaleStage { Name = "Квалификация", SortOrder = 2, TenantId = tId, CreatedAt = now, CreatedById = 1 },
            new SaleStage { Name = "Презентация", SortOrder = 3, TenantId = tId, CreatedAt = now, CreatedById = 1 },
            new SaleStage { Name = "Коммерческое предложение", SortOrder = 4, TenantId = tId, CreatedAt = now, CreatedById = 1 },
            new SaleStage { Name = "Переговоры", SortOrder = 5, TenantId = tId, CreatedAt = now, CreatedById = 1 },
            new SaleStage { Name = "Закрытие сделки", SortOrder = 6, TenantId = tId, CreatedAt = now, CreatedById = 1 }
        );

        context.ClientTypes.AddRange(
            new ClientType { Name = "Физическое лицо", TenantId = tId, CreatedAt = now, CreatedById = 1 },
            new ClientType { Name = "Юридическое лицо", TenantId = tId, CreatedAt = now, CreatedById = 1 }
        );

        context.ClientDocumentTypes.AddRange(
            new ClientDocumentType { Name = "Паспорт РФ", TenantId = tId, CreatedAt = now, CreatedById = 1 },
            new ClientDocumentType { Name = "Загранпаспорт", TenantId = tId, CreatedAt = now, CreatedById = 1 },
            new ClientDocumentType { Name = "Водительское удостоверение", TenantId = tId, CreatedAt = now, CreatedById = 1 }
        );
    }
}