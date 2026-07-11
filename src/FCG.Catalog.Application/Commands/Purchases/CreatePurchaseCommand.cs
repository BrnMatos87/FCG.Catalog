namespace FCG.Catalog.Application.Commands.Purchases;

public class CreatePurchaseCommand
{
    public Guid UserId { get; set; }

    public Guid GameId { get; set; }

    public string UserEmail { get; set; } = string.Empty;
}