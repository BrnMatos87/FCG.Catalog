using FCG.Catalog.Domain.Entities;
using FCG.Catalog.Domain.Enums;
using FCG.Catalog.Domain.Exceptions;

namespace FCG.Catalog.Tests.Domain.Entities;

public class GameLibraryTests
{
    [Fact(DisplayName = "Validando criação de compra pendente com usuário vazio")]
    [Trait("Categoria", "Validando Biblioteca")]
    public void GameLibrary_Validate_UserId_Empty()
    {
        var result = Assert.Throws<DomainException>(() =>
            GameLibrary.CreatePendingPurchase(
                Guid.Empty,
                Guid.NewGuid(),
                199));

        Assert.Equal("O identificador do usuário é obrigatório.", result.Message);
    }

    [Fact(DisplayName = "Validando criação de compra pendente com jogo vazio")]
    [Trait("Categoria", "Validando Biblioteca")]
    public void GameLibrary_Validate_GameId_Empty()
    {
        var result = Assert.Throws<DomainException>(() =>
            GameLibrary.CreatePendingPurchase(
                Guid.NewGuid(),
                Guid.Empty,
                199));

        Assert.Equal("O identificador do jogo é obrigatório.", result.Message);
    }

    [Fact(DisplayName = "Validando criação de compra pendente com preço zero")]
    [Trait("Categoria", "Validando Biblioteca")]
    public void GameLibrary_Validate_Price_Zero()
    {
        var result = Assert.Throws<DomainException>(() =>
            GameLibrary.CreatePendingPurchase(
                Guid.NewGuid(),
                Guid.NewGuid(),
                0));

        Assert.Equal("O preço da compra deve ser maior que zero.", result.Message);
    }

    [Fact(DisplayName = "Validando criação de compra pendente com preço negativo")]
    [Trait("Categoria", "Validando Biblioteca")]
    public void GameLibrary_Validate_Price_Negative()
    {
        var result = Assert.Throws<DomainException>(() =>
            GameLibrary.CreatePendingPurchase(
                Guid.NewGuid(),
                Guid.NewGuid(),
                -10));

        Assert.Equal("O preço da compra deve ser maior que zero.", result.Message);
    }

    [Fact(DisplayName = "Validando criação de compra pendente com sucesso")]
    [Trait("Categoria", "Validando Biblioteca")]
    public void GameLibrary_CreatePendingPurchase_Success()
    {
        var userId = Guid.NewGuid();
        var gameId = Guid.NewGuid();

        var purchase = GameLibrary.CreatePendingPurchase(
            userId,
            gameId,
            199);

        Assert.NotEqual(Guid.Empty, purchase.Id);
        Assert.NotEqual(Guid.Empty, purchase.OrderId);
        Assert.Equal(userId, purchase.UserId);
        Assert.Equal(gameId, purchase.GameId);
        Assert.Equal(199, purchase.Price);
        Assert.Equal(PurchaseStatus.Pending, purchase.PurchaseStatus);
        Assert.True(purchase.IsPending());
        Assert.False(purchase.IsApproved());
        Assert.False(purchase.IsRejected());
        Assert.Null(purchase.PurchasedAt);
        Assert.NotEqual(default, purchase.CreatedAt);
        Assert.Null(purchase.UpdatedAt);
    }

    [Fact(DisplayName = "Validando aprovação de compra")]
    [Trait("Categoria", "Validando Biblioteca")]
    public void GameLibrary_Approve_Success()
    {
        var purchase = CreatePendingPurchase();

        purchase.Approve();

        Assert.Equal(PurchaseStatus.Approved, purchase.PurchaseStatus);
        Assert.True(purchase.IsApproved());
        Assert.False(purchase.IsPending());
        Assert.False(purchase.IsRejected());
        Assert.NotNull(purchase.PurchasedAt);
        Assert.NotNull(purchase.UpdatedAt);
    }

    [Fact(DisplayName = "Validando rejeição de compra")]
    [Trait("Categoria", "Validando Biblioteca")]
    public void GameLibrary_Reject_Success()
    {
        var purchase = CreatePendingPurchase();

        purchase.Reject();

        Assert.Equal(PurchaseStatus.Rejected, purchase.PurchaseStatus);
        Assert.True(purchase.IsRejected());
        Assert.False(purchase.IsPending());
        Assert.False(purchase.IsApproved());
        Assert.Null(purchase.PurchasedAt);
        Assert.NotNull(purchase.UpdatedAt);
    }

    [Fact(DisplayName = "Validando aprovação de compra já aprovada")]
    [Trait("Categoria", "Validando Biblioteca")]
    public void GameLibrary_Approve_WhenAlreadyApproved()
    {
        var purchase = CreatePendingPurchase();

        purchase.Approve();

        var result = Assert.Throws<DomainException>(() =>
            purchase.Approve());

        Assert.Equal("A compra não está pendente.", result.Message);
    }

    [Fact(DisplayName = "Validando rejeição de compra já rejeitada")]
    [Trait("Categoria", "Validando Biblioteca")]
    public void GameLibrary_Reject_WhenAlreadyRejected()
    {
        var purchase = CreatePendingPurchase();

        purchase.Reject();

        var result = Assert.Throws<DomainException>(() =>
            purchase.Reject());

        Assert.Equal("A compra não está pendente.", result.Message);
    }

    [Fact(DisplayName = "Validando rejeição de compra aprovada")]
    [Trait("Categoria", "Validando Biblioteca")]
    public void GameLibrary_Reject_WhenAlreadyApproved()
    {
        var purchase = CreatePendingPurchase();

        purchase.Approve();

        var result = Assert.Throws<DomainException>(() =>
            purchase.Reject());

        Assert.Equal("A compra não está pendente.", result.Message);
    }

    [Fact(DisplayName = "Validando aprovação de compra rejeitada")]
    [Trait("Categoria", "Validando Biblioteca")]
    public void GameLibrary_Approve_WhenAlreadyRejected()
    {
        var purchase = CreatePendingPurchase();

        purchase.Reject();

        var result = Assert.Throws<DomainException>(() =>
            purchase.Approve());

        Assert.Equal("A compra não está pendente.", result.Message);
    }

    private static GameLibrary CreatePendingPurchase()
    {
        return GameLibrary.CreatePendingPurchase(
            Guid.NewGuid(),
            Guid.NewGuid(),
            199);
    }
}