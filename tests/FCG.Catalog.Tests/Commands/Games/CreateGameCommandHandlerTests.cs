using FCG.Catalog.Application.Commands.Games;
using FCG.Catalog.Application.Commands.Games.Handlers;
using FCG.Catalog.Application.Contracts;
using FCG.Catalog.Domain.Entities;
using Moq;

namespace FCG.Catalog.Tests.Application.Commands.Games;

public class CreateGameCommandHandlerTests
{
    private readonly Mock<IGameRepository> _gameRepositoryMock;
    private readonly CreateGameCommandHandler _handler;

    public CreateGameCommandHandlerTests()
    {
        _gameRepositoryMock = new Mock<IGameRepository>();
        _handler = new CreateGameCommandHandler(_gameRepositoryMock.Object);
    }

    [Fact(DisplayName = "Validando criação de jogo com sucesso")]
    [Trait("Categoria", "Application - CreateGame")]
    public async Task CreateGame_HandleAsync_Success()
    {
        var command = new CreateGameCommand
        {
            Title = "Sonic",
            Description = "Jogo de aventura",
            Price = 199,
            Category = "Aventura"
        };

        _gameRepositoryMock
            .Setup(x => x.GetByTitleAsync(command.Title, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Game?)null);

        var result = await _handler.HandleAsync(command);

        Assert.NotEqual(Guid.Empty, result);

        _gameRepositoryMock.Verify(
            x => x.CreateAsync(
                It.Is<Game>(g =>
                    g.Title == command.Title &&
                    g.Description == command.Description &&
                    g.Price == command.Price &&
                    g.Category == command.Category),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact(DisplayName = "Validando criação de jogo com título já cadastrado")]
    [Trait("Categoria", "Application - CreateGame")]
    public async Task CreateGame_HandleAsync_TitleAlreadyExists()
    {
        var command = new CreateGameCommand
        {
            Title = "Sonic",
            Description = "Jogo de aventura",
            Price = 199,
            Category = "Aventura"
        };

        var existingGame = Game.Create(
            command.Title,
            command.Description,
            command.Price,
            command.Category);

        _gameRepositoryMock
            .Setup(x => x.GetByTitleAsync(command.Title, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingGame);

        var result = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _handler.HandleAsync(command));

        Assert.Equal("Já existe um jogo cadastrado com este título.", result.Message);

        _gameRepositoryMock.Verify(
            x => x.CreateAsync(It.IsAny<Game>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
}