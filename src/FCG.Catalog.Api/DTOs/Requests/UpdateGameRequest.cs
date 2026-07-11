namespace FCG.Catalog.Api.DTOs.Requests;

public class UpdateGameRequest
{
    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public decimal Price { get; set; }

    public string Category { get; set; } = string.Empty;
}