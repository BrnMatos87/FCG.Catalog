namespace FCG.Catalog.Api.DTOs.Requests;

public class CreatePurchaseRequest
{
    public Guid UserId { get; set; }

    public Guid GameId { get; set; }

    public string UserEmail { get; set; } = string.Empty;
}