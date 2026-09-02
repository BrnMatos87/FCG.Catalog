using FCG.BuildingBlocks.Enums;
using FCG.Catalog.Application.Contracts;
using FCG.Catalog.Application.Queries.Games;
using FCG.Catalog.Application.Queries.Games.Handlers;
using FCG.Catalog.Application.Responses;
using FCG.Catalog.Domain.Entities;
using Moq;

namespace FCG.Catalog.Tests.Application.Queries.Games;

public class GetGameByIdQueryHandlerTests
{
    private readonly Mock<IGameRepository> _gameRepositoryMock;
    private readonly Mock<IGameCache> _gameCacheMock;
    private readonly GetGameByIdQueryHandler _handler;

    public GetGameByIdQueryHandlerTests()
    {
        _gameRepositoryMock = new Mock<IGameRepository>();
        _gameCacheMock = new Mock<IGameCache>();
        _handler = new GetGameByIdQueryHandler(_gameRepositoryMock.Object, _gameCacheMock.Object);
    }

    [Fact(DisplayName = "Validando busca de jogo por id com sucesso")]
    [Trait("Categoria", "Application - GetGameById")]
    public async Task GetGameById_HandleAsync_Success()
    {
        var game = Game.Create(
            "Sonic",
            "Jogo de aventura",
            199,
            "Aventura");

        var query = new GetGameByIdQuery
        {
            Id = game.Id
        };

        _gameCacheMock
            .Setup(x => x.GetByIdAsync(query.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((GameResponse?)null);

        _gameRepositoryMock
            .Setup(x => x.GetByIdAsync(query.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(game);

        var response = await _handler.HandleAsync(query);

        Assert.NotNull(response);
        Assert.Equal(game.Id, response!.Id);
        Assert.Equal(game.Title, response.Title);
        Assert.Equal(game.Description, response.Description);
        Assert.Equal(game.Price, response.Price);
        Assert.Equal(game.Category, response.Category);
        Assert.Equal(StatusType.Active, response.Status);
        Assert.Equal(game.CreatedAt, response.CreatedAt);
        Assert.Equal(game.UpdatedAt, response.UpdatedAt);

        _gameCacheMock.Verify(
            x => x.SetByIdAsync(
                It.Is<GameResponse>(r =>
                    r.Id == game.Id &&
                    r.Title == game.Title &&
                    r.Description == game.Description &&
                    r.Price == game.Price &&
                    r.Category == game.Category &&
                    r.Status == game.Status &&
                    r.CreatedAt == game.CreatedAt &&
                    r.UpdatedAt == game.UpdatedAt),
                TimeSpan.FromMinutes(5),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact(DisplayName = "Validando busca de jogo por id inexistente")]
    [Trait("Categoria", "Application - GetGameById")]
    public async Task GetGameById_HandleAsync_NotFound()
    {
        var query = new GetGameByIdQuery
        {
            Id = Guid.NewGuid()
        };

        _gameCacheMock
            .Setup(x => x.GetByIdAsync(query.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((GameResponse?)null);

        _gameRepositoryMock
            .Setup(x => x.GetByIdAsync(query.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Game?)null);

        var response = await _handler.HandleAsync(query);

        Assert.Null(response);

        _gameCacheMock.Verify(
            x => x.SetByIdAsync(
                It.IsAny<GameResponse>(),
                It.IsAny<TimeSpan>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }
}