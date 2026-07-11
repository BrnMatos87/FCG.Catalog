namespace FCG.Catalog.Application.Commands.Games;

public class CreateGameCommand
{
    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public decimal Price { get; set; }

    public string Category { get; set; } = string.Empty;
}