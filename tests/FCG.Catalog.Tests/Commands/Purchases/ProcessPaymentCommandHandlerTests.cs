using FCG.BuildingBlocks.Enums;
using FCG.Catalog.Application.Commands.Purchases;
using FCG.Catalog.Application.Commands.Purchases.Handlers;
using FCG.Catalog.Application.Contracts;
using FCG.Catalog.Domain.Entities;
using FCG.Catalog.Domain.Enums;
using Moq;

namespace FCG.Catalog.Tests.Application.Commands.Purchases;

public class ProcessPaymentCommandHandlerTests
{
    private readonly Mock<IGameLibraryRepository> _gameLibraryRepositoryMock;
    private readonly ProcessPaymentCommandHandler _handler;

    public ProcessPaymentCommandHandlerTests()
    {
        _gameLibraryRepositoryMock = new Mock<IGameLibraryRepository>();
        _handler = new ProcessPaymentCommandHandler(_gameLibraryRepositoryMock.Object);
    }

    [Fact(DisplayName = "Validando processamento de pagamento aprovado")]
    [Trait("Categoria", "Application - ProcessPayment")]
    public async Task ProcessPayment_HandleAsync_Approved()
    {
        var purchase = CreatePendingPurchase();

        var command = new ProcessPaymentCommand
        {
            OrderId = purchase.OrderId,
            UserId = purchase.UserId,
            GameId = purchase.GameId,
            UserEmail = "usuario@email.com",
            GameTitle = "Sonic",
            Price = purchase.Price,
            Status = PaymentStatus.Approved,
            CorrelationId = Guid.NewGuid()
        };

        _gameLibraryRepositoryMock
            .Setup(x => x.GetByOrderIdAsync(command.OrderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(purchase);

        await _handler.HandleAsync(command);

        Assert.Equal(PurchaseStatus.Approved, purchase.PurchaseStatus);
        Assert.NotNull(purchase.PurchasedAt);
        Assert.NotNull(purchase.UpdatedAt);

        _gameLibraryRepositoryMock.Verify(
            x => x.UpdateAsync(purchase, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact(DisplayName = "Validando processamento de pagamento rejeitado")]
    [Trait("Categoria", "Application - ProcessPayment")]
    public async Task ProcessPayment_HandleAsync_Rejected()
    {
        var purchase = CreatePendingPurchase();

        var command = new ProcessPaymentCommand
        {
            OrderId = purchase.OrderId,
            UserId = purchase.UserId,
            GameId = purchase.GameId,
            UserEmail = "usuario@email.com",
            GameTitle = "Sonic",
            Price = purchase.Price,
            Status = PaymentStatus.Rejected,
            CorrelationId = Guid.NewGuid()
        };

        _gameLibraryRepositoryMock
            .Setup(x => x.GetByOrderIdAsync(command.OrderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(purchase);

        await _handler.HandleAsync(command);

        Assert.Equal(PurchaseStatus.Rejected, purchase.PurchaseStatus);
        Assert.Null(purchase.PurchasedAt);
        Assert.NotNull(purchase.UpdatedAt);

        _gameLibraryRepositoryMock.Verify(
            x => x.UpdateAsync(purchase, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact(DisplayName = "Validando processamento de pagamento com pedido inexistente")]
    [Trait("Categoria", "Application - ProcessPayment")]
    public async Task ProcessPayment_HandleAsync_OrderNotFound()
    {
        var command = new ProcessPaymentCommand
        {
            OrderId = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            GameId = Guid.NewGuid(),
            UserEmail = "usuario@email.com",
            GameTitle = "Sonic",
            Price = 199,
            Status = PaymentStatus.Approved,
            CorrelationId = Guid.NewGuid()
        };

        _gameLibraryRepositoryMock
            .Setup(x => x.GetByOrderIdAsync(command.OrderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((GameLibrary?)null);

        var result = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _handler.HandleAsync(command));

        Assert.Equal("Pedido de compra não encontrado.", result.Message);

        _gameLibraryRepositoryMock.Verify(
            x => x.UpdateAsync(It.IsAny<GameLibrary>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact(DisplayName = "Validando processamento de pagamento com status inválido")]
    [Trait("Categoria", "Application - ProcessPayment")]
    public async Task ProcessPayment_HandleAsync_InvalidStatus()
    {
        var purchase = CreatePendingPurchase();

        var command = new ProcessPaymentCommand
        {
            OrderId = purchase.OrderId,
            UserId = purchase.UserId,
            GameId = purchase.GameId,
            UserEmail = "usuario@email.com",
            GameTitle = "Sonic",
            Price = purchase.Price,
            Status = (PaymentStatus)999,
            CorrelationId = Guid.NewGuid()
        };

        _gameLibraryRepositoryMock
            .Setup(x => x.GetByOrderIdAsync(command.OrderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(purchase);

        var result = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _handler.HandleAsync(command));

        Assert.Equal("Status de pagamento inválido.", result.Message);

        _gameLibraryRepositoryMock.Verify(
            x => x.UpdateAsync(It.IsAny<GameLibrary>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    private static GameLibrary CreatePendingPurchase()
    {
        return GameLibrary.CreatePendingPurchase(
            Guid.NewGuid(),
            Guid.NewGuid(),
            199);
    }
}