namespace FCG.Catalog.Application.Queries.Games;

public sealed class SearchGamesQuery
{
    public string Term { get; init; } = string.Empty;

    public int Page { get; init; } = 1;

    public int PageSize { get; init; } = 10;
}
