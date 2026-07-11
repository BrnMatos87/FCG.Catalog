using FCG.BuildingBlocks.Enums;

namespace FCG.Catalog.Application.Commands.Purchases;

public class ProcessPaymentCommand
{
    public Guid OrderId { get; set; }

    public Guid UserId { get; set; }

    public Guid GameId { get; set; }

    public string UserEmail { get; set; } = string.Empty;

    public string GameTitle { get; set; } = string.Empty;

    public decimal Price { get; set; }

    public PaymentStatus Status { get; set; }

    public Guid CorrelationId { get; set; }
}