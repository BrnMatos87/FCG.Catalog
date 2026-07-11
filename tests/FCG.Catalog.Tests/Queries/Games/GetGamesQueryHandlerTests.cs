using FCG.BuildingBlocks.Enums;
using FCG.Catalog.Application.Contracts;
using FCG.Catalog.Application.Queries.Games;
using FCG.Catalog.Application.Queries.Games.Handlers;
using FCG.Catalog.Domain.Entities;
using Moq;

namespace FCG.Catalog.Tests.Application.Queries.Games;

public class GetGamesQueryHandlerTests
{
    private readonly Mock<IGameRepository> _gameRepositoryMock;
    private readonly GetGamesQueryHandler _handler;

    public GetGamesQueryHandlerTests()
    {
        _gameRepositoryMock = new Mock<IGameRepository>();
        _handler = new GetGamesQueryHandler(_gameRepositoryMock.Object);
    }

    [Fact(DisplayName = "Validando busca de jogos com sucesso")]
    [Trait("Categoria", "Application - GetGames")]
    public async Task GetGames_HandleAsync_Success()
    {
        var games = new List<Game>
        {
            Game.Create("Sonic", "Jogo de aventura", 199, "Aventura"),
            Game.Create("Mario", "Jogo de plataforma", 250, "Plataforma")
        };

        _gameRepositoryMock
            .Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(games);

        var response = await _handler.HandleAsync(new GetGamesQuery());

        Assert.NotNull(response);
        Assert.Equal(2, response.Count);

        Assert.Contains(response, x =>
            x.Title == "Sonic" &&
            x.Description == "Jogo de aventura" &&
            x.Price == 199 &&
            x.Category == "Aventura" &&
            x.Status == StatusType.Active);

        Assert.Contains(response, x =>
            x.Title == "Mario" &&
            x.Description == "Jogo de plataforma" &&
            x.Price == 250 &&
            x.Category == "Plataforma" &&
            x.Status == StatusType.Active);
    }

    [Fact(DisplayName = "Validando busca de jogos sem resultados")]
    [Trait("Categoria", "Application - GetGames")]
    public async Task GetGames_HandleAsync_Empty()
    {
        _gameRepositoryMock
            .Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Game>());

        var response = await _handler.HandleAsync(new GetGamesQuery());

        Assert.NotNull(response);
        Assert.Empty(response);
    }
}