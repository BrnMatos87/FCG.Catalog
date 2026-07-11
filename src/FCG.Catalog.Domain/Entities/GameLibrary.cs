using FCG.BuildingBlocks.Domain;
using FCG.Catalog.Domain.Enums;
using FCG.Catalog.Domain.Exceptions;

namespace FCG.Catalog.Domain.Entities;

public class GameLibrary : EntityBase
{
    public Guid OrderId { get; private set; }

    public Guid UserId { get; private set; }

    public Guid GameId { get; private set; }

    public decimal Price { get; private set; }

    public PurchaseStatus PurchaseStatus { get; private set; }

    public DateTime? PurchasedAt { get; private set; }

    protected GameLibrary()
    {
    }

    public static GameLibrary CreatePendingPurchase(
        Guid userId,
        Guid gameId,
        decimal price)
    {
        var orderId = Guid.NewGuid();

        ValidateOrderId(orderId);
        ValidateUserId(userId);
        ValidateGameId(gameId);
        ValidatePrice(price);

        return new GameLibrary
        {
            Id = Guid.NewGuid(),
            OrderId = orderId,
            UserId = userId,
            GameId = gameId,
            Price = price,
            PurchaseStatus = PurchaseStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void Approve()
    {
        EnsurePending();

        PurchaseStatus = PurchaseStatus.Approved;
        PurchasedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Reject()
    {
        EnsurePending();

        PurchaseStatus = PurchaseStatus.Rejected;
        UpdatedAt = DateTime.UtcNow;
    }

    public bool IsApproved()
    {
        return PurchaseStatus == PurchaseStatus.Approved;
    }

    public bool IsPending()
    {
        return PurchaseStatus == PurchaseStatus.Pending;
    }

    public bool IsRejected()
    {
        return PurchaseStatus == PurchaseStatus.Rejected;
    }

    private void EnsurePending()
    {
        if (PurchaseStatus != PurchaseStatus.Pending)
            throw new DomainException("A compra não está pendente.");
    }

    private static void ValidateOrderId(Guid orderId)
    {
        if (orderId == Guid.Empty)
            throw new DomainException("O identificador do pedido é obrigatório.");
    }

    private static void ValidateUserId(Guid userId)
    {
        if (userId == Guid.Empty)
            throw new DomainException("O identificador do usuário é obrigatório.");
    }

    private static void ValidateGameId(Guid gameId)
    {
        if (gameId == Guid.Empty)
            throw new DomainException("O identificador do jogo é obrigatório.");
    }

    private static void ValidatePrice(decimal price)
    {
        if (price <= 0)
            throw new DomainException("O preço da compra deve ser maior que zero.");
    }
}