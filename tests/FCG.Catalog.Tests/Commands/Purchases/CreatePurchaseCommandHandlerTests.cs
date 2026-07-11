using FCG.Catalog.Application.Commands.Purchases;
using FCG.Catalog.Application.Commands.Purchases.Handlers;
using FCG.Catalog.Application.Contracts;
using FCG.Catalog.Domain.Entities;
using Moq;

namespace FCG.Catalog.Tests.Application.Commands.Purchases;

public class CreatePurchaseCommandHandlerTests
{
    private readonly Mock<IGameRepository> _gameRepositoryMock;
    private readonly Mock<IGameLibraryRepository> _gameLibraryRepositoryMock;
    private readonly Mock<ICatalogEventPublisher> _eventPublisherMock;
    private readonly Mock<ICorrelationIdAccessor> _correlationIdAccessorMock;
    private readonly CreatePurchaseCommandHandler _handler;

    public CreatePurchaseCommandHandlerTests()
    {
        _gameRepositoryMock = new Mock<IGameRepository>();
        _gameLibraryRepositoryMock = new Mock<IGameLibraryRepository>();
        _eventPublisherMock = new Mock<ICatalogEventPublisher>();
        _correlationIdAccessorMock = new Mock<ICorrelationIdAccessor>();

        _handler = new CreatePurchaseCommandHandler(
            _gameRepositoryMock.Object,
            _gameLibraryRepositoryMock.Object,
            _eventPublisherMock.Object,
            _correlationIdAccessorMock.Object);
    }

    [Fact(DisplayName = "Validando criação de compra com sucesso")]
    [Trait("Categoria", "Application - CreatePurchase")]
    public async Task CreatePurchase_HandleAsync_Success()
    {
        var game = Game.Create(
            "Sonic",
            "Jogo de aventura",
            199,
            "Aventura");

        var command = new CreatePurchaseCommand
        {
            UserId = Guid.NewGuid(),
            GameId = game.Id,
            UserEmail = "usuario@email.com"
        };

        var correlationId = Guid.NewGuid();

        _gameRepositoryMock
            .Setup(x => x.GetByIdAsync(command.GameId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(game);

        _gameLibraryRepositoryMock
            .Setup(x => x.GetByUserIdAndGameIdAsync(
                command.UserId,
                command.GameId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((GameLibrary?)null);

        _correlationIdAccessorMock
            .Setup(x => x.Get())
            .Returns(correlationId);

        var result = await _handler.HandleAsync(command);

        Assert.NotEqual(Guid.Empty, result);

        _gameLibraryRepositoryMock.Verify(
            x => x.CreateAsync(
                It.Is<GameLibrary>(purchase =>
                    purchase.OrderId == result &&
                    purchase.UserId == command.UserId &&
                    purchase.GameId == command.GameId &&
                    purchase.Price == game.Price &&
                    purchase.IsPending()),
                It.IsAny<CancellationToken>()),
            Times.Once);

        _eventPublisherMock.Verify(
            x => x.PublishOrderPlacedAsync(
                It.Is<FCG.BuildingBlocks.Events.OrderPlacedEvent>(@event =>
                    @event.OrderId == result &&
                    @event.UserId == command.UserId &&
                    @event.GameId == command.GameId &&
                    @event.UserEmail == command.UserEmail &&
                    @event.GameTitle == game.Title &&
                    @event.Price == game.Price &&
                    @event.CorrelationId == correlationId),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact(DisplayName = "Validando criação de compra com jogo inexistente")]
    [Trait("Categoria", "Application - CreatePurchase")]
    public async Task CreatePurchase_HandleAsync_GameNotFound()
    {
        var command = new CreatePurchaseCommand
        {
            UserId = Guid.NewGuid(),
            GameId = Guid.NewGuid(),
            UserEmail = "usuario@email.com"
        };

        _gameRepositoryMock
            .Setup(x => x.GetByIdAsync(command.GameId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Game?)null);

        var result = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _handler.HandleAsync(command));

        Assert.Equal("Jogo não encontrado.", result.Message);

        _gameLibraryRepositoryMock.Verify(
            x => x.CreateAsync(It.IsAny<GameLibrary>(), It.IsAny<CancellationToken>()),
            Times.Never);

        _eventPublisherMock.Verify(
            x => x.PublishOrderPlacedAsync(
                It.IsAny<FCG.BuildingBlocks.Events.OrderPlacedEvent>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact(DisplayName = "Validando criação de compra com jogo inativo")]
    [Trait("Categoria", "Application - CreatePurchase")]
    public async Task CreatePurchase_HandleAsync_GameInactive()
    {
        var game = Game.Create(
            "Sonic",
            "Jogo de aventura",
            199,
            "Aventura");

        game.Inactivate();

        var command = new CreatePurchaseCommand
        {
            UserId = Guid.NewGuid(),
            GameId = game.Id,
            UserEmail = "usuario@email.com"
        };

        _gameRepositoryMock
            .Setup(x => x.GetByIdAsync(command.GameId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(game);

        var result = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _handler.HandleAsync(command));

        Assert.Equal("Jogo inativo.", result.Message);

        _gameLibraryRepositoryMock.Verify(
            x => x.CreateAsync(It.IsAny<GameLibrary>(), It.IsAny<CancellationToken>()),
            Times.Never);

        _eventPublisherMock.Verify(
            x => x.PublishOrderPlacedAsync(
                It.IsAny<FCG.BuildingBlocks.Events.OrderPlacedEvent>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact(DisplayName = "Validando criação de compra para jogo já adquirido")]
    [Trait("Categoria", "Application - CreatePurchase")]
    public async Task CreatePurchase_HandleAsync_UserAlreadyOwnsGame()
    {
        var game = Game.Create(
            "Sonic",
            "Jogo de aventura",
            199,
            "Aventura");

        var existingPurchase = GameLibrary.CreatePendingPurchase(
            Guid.NewGuid(),
            game.Id,
            game.Price);

        existingPurchase.Approve();

        var command = new CreatePurchaseCommand
        {
            UserId = existingPurchase.UserId,
            GameId = game.Id,
            UserEmail = "usuario@email.com"
        };

        _gameRepositoryMock
            .Setup(x => x.GetByIdAsync(command.GameId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(game);

        _gameLibraryRepositoryMock
            .Setup(x => x.GetByUserIdAndGameIdAsync(
                command.UserId,
                command.GameId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingPurchase);

        var result = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _handler.HandleAsync(command));

        Assert.Equal("Usuário já possui este jogo.", result.Message);

        _gameLibraryRepositoryMock.Verify(
            x => x.CreateAsync(It.IsAny<GameLibrary>(), It.IsAny<CancellationToken>()),
            Times.Never);

        _eventPublisherMock.Verify(
            x => x.PublishOrderPlacedAsync(
                It.IsAny<FCG.BuildingBlocks.Events.OrderPlacedEvent>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }
}