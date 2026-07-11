using FCG.BuildingBlocks.Enums;
using FCG.Catalog.Application.Contracts;
using FCG.Catalog.Application.Queries.Library;
using FCG.Catalog.Application.Queries.Library.Handlers;
using FCG.Catalog.Domain.Entities;
using FCG.Catalog.Domain.Enums;
using Moq;

namespace FCG.Catalog.Tests.Application.Queries.Library;

public class GetUserLibraryQueryHandlerTests
{
    private readonly Mock<IGameRepository> _gameRepositoryMock;
    private readonly Mock<IGameLibraryRepository> _gameLibraryRepositoryMock;
    private readonly GetUserLibraryQueryHandler _handler;

    public GetUserLibraryQueryHandlerTests()
    {
        _gameRepositoryMock = new Mock<IGameRepository>();
        _gameLibraryRepositoryMock = new Mock<IGameLibraryRepository>();

        _handler = new GetUserLibraryQueryHandler(
            _gameRepositoryMock.Object,
            _gameLibraryRepositoryMock.Object);
    }

    [Fact(DisplayName = "Validando busca da biblioteca do usuário com sucesso")]
    [Trait("Categoria", "Application - GetUserLibrary")]
    public async Task GetUserLibrary_HandleAsync_Success()
    {
        var userId = Guid.NewGuid();

        var game = Game.Create(
            "Sonic",
            "Jogo de aventura",
            199,
            "Aventura");

        var libraryItem = GameLibrary.CreatePendingPurchase(
            userId,
            game.Id,
            game.Price);

        libraryItem.Approve();

        var query = new GetUserLibraryQuery
        {
            UserId = userId
        };

        _gameLibraryRepositoryMock
            .Setup(x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<GameLibrary> { libraryItem });

        _gameRepositoryMock
            .Setup(x => x.GetByIdAsync(game.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(game);

        var response = await _handler.HandleAsync(query);

        Assert.NotNull(response);
        Assert.Single(response);

        var item = response.First();

        Assert.Equal(libraryItem.Id, item.Id);
        Assert.Equal(libraryItem.OrderId, item.OrderId);
        Assert.Equal(userId, item.UserId);
        Assert.Equal(game.Id, item.GameId);
        Assert.Equal(game.Title, item.GameTitle);
        Assert.Equal(game.Price, item.Price);
        Assert.Equal(PurchaseStatus.Approved, item.PurchaseStatus);
        Assert.Equal(StatusType.Active, item.Status);
        Assert.Equal(libraryItem.PurchasedAt, item.PurchasedAt);
        Assert.Equal(libraryItem.CreatedAt, item.CreatedAt);
    }

    [Fact(DisplayName = "Validando busca da biblioteca do usuário sem resultados")]
    [Trait("Categoria", "Application - GetUserLibrary")]
    public async Task GetUserLibrary_HandleAsync_Empty()
    {
        var userId = Guid.NewGuid();

        var query = new GetUserLibraryQuery
        {
            UserId = userId
        };

        _gameLibraryRepositoryMock
            .Setup(x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<GameLibrary>());

        var response = await _handler.HandleAsync(query);

        Assert.NotNull(response);
        Assert.Empty(response);
    }

    [Fact(DisplayName = "Validando busca da biblioteca com jogo não encontrado")]
    [Trait("Categoria", "Application - GetUserLibrary")]
    public async Task GetUserLibrary_HandleAsync_GameNotFound()
    {
        var userId = Guid.NewGuid();
        var gameId = Guid.NewGuid();

        var libraryItem = GameLibrary.CreatePendingPurchase(
            userId,
            gameId,
            199);

        libraryItem.Approve();

        var query = new GetUserLibraryQuery
        {
            UserId = userId
        };

        _gameLibraryRepositoryMock
            .Setup(x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<GameLibrary> { libraryItem });

        _gameRepositoryMock
            .Setup(x => x.GetByIdAsync(gameId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Game?)null);

        var response = await _handler.HandleAsync(query);

        Assert.NotNull(response);
        Assert.Single(response);

        var item = response.First();

        Assert.Equal(string.Empty, item.GameTitle);
        Assert.Equal(libraryItem.Price, item.Price);
    }
}