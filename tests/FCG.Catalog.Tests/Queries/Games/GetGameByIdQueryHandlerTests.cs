using FCG.BuildingBlocks.Enums;
using FCG.Catalog.Application.Contracts;
using FCG.Catalog.Application.Queries.Games;
using FCG.Catalog.Application.Queries.Games.Handlers;
using FCG.Catalog.Domain.Entities;
using Moq;

namespace FCG.Catalog.Tests.Application.Queries.Games;

public class GetGameByIdQueryHandlerTests
{
    private readonly Mock<IGameRepository> _gameRepositoryMock;
    private readonly GetGameByIdQueryHandler _handler;

    public GetGameByIdQueryHandlerTests()
    {
        _gameRepositoryMock = new Mock<IGameRepository>();
        _handler = new GetGameByIdQueryHandler(_gameRepositoryMock.Object);
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
    }

    [Fact(DisplayName = "Validando busca de jogo por id inexistente")]
    [Trait("Categoria", "Application - GetGameById")]
    public async Task GetGameById_HandleAsync_NotFound()
    {
        var query = new GetGameByIdQuery
        {
            Id = Guid.NewGuid()
        };

        _gameRepositoryMock
            .Setup(x => x.GetByIdAsync(query.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Game?)null);

        var response = await _handler.HandleAsync(query);

        Assert.Null(response);
    }
}