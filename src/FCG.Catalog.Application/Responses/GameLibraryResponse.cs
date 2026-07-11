using FCG.BuildingBlocks.Enums;
using FCG.Catalog.Domain.Enums;

namespace FCG.Catalog.Application.Responses;

public class GameLibraryResponse
{
    public Guid Id { get; set; }

    public Guid OrderId { get; set; }

    public Guid UserId { get; set; }

    public Guid GameId { get; set; }

    public string GameTitle { get; set; } = string.Empty;

    public decimal Price { get; set; }

    public PurchaseStatus PurchaseStatus { get; set; }

    public StatusType Status { get; set; }

    public DateTime? PurchasedAt { get; set; }

    public DateTime CreatedAt { get; set; }
}