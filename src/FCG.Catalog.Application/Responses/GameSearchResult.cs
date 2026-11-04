namespace FCG.Catalog.Application.Responses;

public sealed class GameSearchResult
{
    public Guid Id { get; init; }

    public string Title { get; init; } = string.Empty;

    public string Description { get; init; } = string.Empty;

    public decimal Price { get; init; }

    public string Category { get; init; } = string.Empty;

    public double Relevance { get; init; }
}
