namespace FCG.Catalog.Infrastructure.Search;

public sealed class GameSearchDocument
{
    public Guid Id { get; init; }

    public string Title { get; init; } = string.Empty;

    public string Description { get; init; } = string.Empty;

    public decimal Price { get; init; }

    public string Category { get; init; } = string.Empty;

    public bool Active { get; init; }
}
