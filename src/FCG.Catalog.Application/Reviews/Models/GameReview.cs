namespace FCG.Catalog.Application.Reviews.Models;

public class GameReview
{
    public string Id { get; set; } = string.Empty;

    public Guid GameId { get; set; }

    public Guid UserId { get; set; }

    public int Rating { get; set; }

    public string? Comment { get; set; }

    public DateTime CreatedAt { get; set; }
}