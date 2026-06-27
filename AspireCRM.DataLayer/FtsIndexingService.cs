using AspireCRM.Domain.Search;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace AspireCRM.DataLayer;

public class FtsIndexingService
{
    private readonly AspireCRMDbContext _db;

    public FtsIndexingService(AspireCRMDbContext db)
    {
        _db = db;
    }

    public async Task RebuildIndexAsync()
    {
        await _db.Database.ExecuteSqlRawAsync("DELETE FROM FtsSearch");
        await _db.Database.ExecuteSqlRawAsync("INSERT INTO FtsSearch(entity_type, entity_id, title, content) SELECT 'Lead', Id, Name, COALESCE(Description, '') FROM Leads WHERE IsDeleted = 0");
        await _db.Database.ExecuteSqlRawAsync("INSERT INTO FtsSearch(entity_type, entity_id, title, content) SELECT 'Contractor', Id, Name, COALESCE(Description, '') FROM Contractors WHERE IsDeleted = 0");
        await _db.Database.ExecuteSqlRawAsync("INSERT INTO FtsSearch(entity_type, entity_id, title, content) SELECT 'Contact', Id, COALESCE(Surname, '') || ' ' || COALESCE(Firstname, '') || ' ' || COALESCE(Middlename, ''), COALESCE(Description, '') FROM Contacts WHERE IsDeleted = 0");
        await _db.Database.ExecuteSqlRawAsync("INSERT INTO FtsSearch(entity_type, entity_id, title, content) SELECT 'Sale', Id, Name, COALESCE(Description, '') FROM Sales WHERE IsDeleted = 0");
        await _db.Database.ExecuteSqlRawAsync("INSERT INTO FtsSearch(entity_type, entity_id, title, content) SELECT 'Inpayment', Id, Name, COALESCE(Comment, '') FROM Inpayments WHERE IsDeleted = 0");
        await _db.Database.ExecuteSqlRawAsync("INSERT INTO FtsSearch(entity_type, entity_id, title, content) SELECT 'Product', Id, Name, '' FROM Products WHERE IsDeleted = 0");
        await _db.Database.ExecuteSqlRawAsync("INSERT INTO FtsSearch(entity_type, entity_id, title, content) SELECT 'Relationship', Id, Theme, COALESCE(Description, '') FROM Relationships WHERE IsDeleted = 0");
        await _db.Database.ExecuteSqlRawAsync("INSERT INTO FtsSearch(entity_type, entity_id, title, content) SELECT 'MarketingActivity', Id, Name, COALESCE(Description, '') FROM MarketingActivities WHERE IsDeleted = 0");
    }

    public async Task<SearchResponse> SearchAsync(string query, int page = 1, int pageSize = 20)
    {
        if (string.IsNullOrWhiteSpace(query))
            return new SearchResponse([], 0);

        var sanitized = query.Replace("'", "''");
        var ftsQuery = $"{sanitized}*";

        var offset = (page - 1) * pageSize;

        var conn = _db.Database.GetDbConnection();
        await conn.OpenAsync();

        await using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            SELECT entity_type, entity_id, title, snippet(FtsSearch, 0, '<b>', '</b>', '...', 32) as snippet
            FROM FtsSearch
            WHERE FtsSearch MATCH @query
            ORDER BY rank
            LIMIT @limit OFFSET @offset";
        cmd.Parameters.Add(new SqliteParameter("@query", ftsQuery));
        cmd.Parameters.Add(new SqliteParameter("@limit", pageSize));
        cmd.Parameters.Add(new SqliteParameter("@offset", offset));

        var results = new List<SearchResultItem>();
        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            results.Add(new SearchResultItem(
                reader.GetString(0),
                reader.GetInt64(1),
                reader.GetString(2),
                reader.IsDBNull(3) ? null : reader.GetString(3)
            ));
        }

        cmd.CommandText = "SELECT COUNT(*) FROM FtsSearch WHERE FtsSearch MATCH @query";
        cmd.Parameters.Clear();
        cmd.Parameters.Add(new SqliteParameter("@query", ftsQuery));
        var totalCount = Convert.ToInt32(await cmd.ExecuteScalarAsync());

        return new SearchResponse(results, totalCount);
    }
}
