using AspireCRM.DataLayer;

namespace AspireCRM.ApiService.Endpoints;

public static class SearchEndpoints
{
    public static void MapSearchEndpoints(this WebApplication app)
    {
        var api = app.MapGroup("/api/search");

        api.MapGet("/", async (string q, int page, int pageSize, FtsIndexingService fts) =>
        {
            if (string.IsNullOrWhiteSpace(q))
                return Results.Ok(new AspireCRM.Domain.Search.SearchResponse([], 0));

            var result = await fts.SearchAsync(q, page, pageSize);
            return Results.Ok(result);
        });
    }
}
