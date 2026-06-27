namespace AspireCRM.Domain.Search;

public record SearchResultItem(
    string EntityType,
    long EntityId,
    string Title,
    string? Snippet
);

public record SearchResponse(
    List<SearchResultItem> Results,
    int TotalCount
);
