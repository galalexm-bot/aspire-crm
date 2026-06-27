using AspireCRM.DataLayer.Repositories;

namespace AspireCRM.ApiService.Endpoints;

public static class LookupEndpoints
{
    public static void MapLookupEndpoints(this WebApplication app)
    {
        var api = app.MapGroup("/api/lookups");

        api.MapGet("/lead-sources", async (IRepository<Domain.Leads.LeadSource> repo) =>
            Results.Ok(await repo.GetAllAsync()));

        api.MapGet("/lead-types", async (IRepository<Domain.Leads.LeadType> repo) =>
            Results.Ok(await repo.GetAllAsync()));

        api.MapGet("/regions", async (IRepository<Domain.Contractors.ContractorRegion> repo) =>
            Results.Ok(await repo.GetAllAsync()));

        api.MapGet("/industries", async (IRepository<Domain.Contractors.ContractorIndustry> repo) =>
            Results.Ok(await repo.GetAllAsync()));

        api.MapGet("/contractor-types", async (IRepository<Domain.Contractors.ContractorType> repo) =>
            Results.Ok(await repo.GetAllAsync()));

        api.MapGet("/legal-forms", async (IRepository<Domain.Contractors.LegalForm> repo) =>
            Results.Ok(await repo.GetAllAsync()));

        api.MapGet("/currencies", async (IRepository<Domain.Sales.Currency> repo) =>
            Results.Ok(await repo.GetAllAsync()));

        api.MapGet("/sale-types", async (IRepository<Domain.Sales.SaleType> repo) =>
            Results.Ok(await repo.GetAllAsync()));

        api.MapGet("/sale-funnels", async (IRepository<Domain.Sales.SaleFunnel> repo) =>
            Results.Ok(await repo.GetAllAsync()));

        api.MapGet("/sale-stages", async (IRepository<Domain.Sales.SaleStage> repo) =>
            Results.Ok(await repo.GetAllAsync()));

        api.MapGet("/client-types", async (IRepository<Domain.Contractors.ClientType> repo) =>
            Results.Ok(await repo.GetAllAsync()));

        api.MapGet("/document-types", async (IRepository<Domain.Contractors.ClientDocumentType> repo) =>
            Results.Ok(await repo.GetAllAsync()));
    }
}