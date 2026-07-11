using FCG.Catalog.Application.Commands.Games;
using FCG.Catalog.Application.Commands.Games.Handlers;
using FCG.Catalog.Application.Contracts;
using FCG.Catalog.Domain.Entities;
using Moq;

namespace FCG.Catalog.Tests.Application.Commands.Games;

public class UpdateGameCommandHandlerTests
{
    private readonly Mock<IGameRepository> _gameRepositoryMock;
    private readonly UpdateGameCommandHandler _handler;

    public UpdateGameCommandHandlerTests()
    {
        _gameRepositoryMock = new Mock<IGameRepository>();
        _handler = new UpdateGameCommandHandler(_gameRepositoryMock.Object);
    }

    [Fact(DisplayName = "Validando atualização de jogo com sucesso")]
    [Trait("Categoria", "Application - UpdateGame")]
    public async Task UpdateGame_HandleAsync_Success()
    {
        var game = Game.Create(
            "Sonic",
            "Jogo de aventura",
            199,
            "Aventura");

        var command = new UpdateGameCommand
        {
            Id = game.Id,
            Title = "Sonic 2",
            Description = "Novo jogo",
            Price = 250,
            Category = "Plataforma"
        };

        _gameRepositoryMock
            .Setup(x => x.GetByIdAsync(command.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(game);

        await _handler.HandleAsync(command);

        _gameRepositoryMock.Verify(
            x => x.UpdateAsync(
                It.Is<Game>(g =>
                    g.Id == command.Id &&
                    g.Title == command.Title &&
                    g.Description == command.Description &&
                    g.Price == command.Price &&
                    g.Category == command.Category),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact(DisplayName = "Validando atualização de jogo inexistente")]
    [Trait("Categoria", "Application - UpdateGame")]
    public async Task UpdateGame_HandleAsync_GameNotFound()
    {
        var command = new UpdateGameCommand
        {
            Id = Guid.NewGuid(),
            Title = "Sonic 2",
            Description = "Novo jogo",
            Price = 250,
            Category = "Plataforma"
        };

        _gameRepositoryMock
            .Setup(x => x.GetByIdAsync(command.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Game?)null);

        var result = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _handler.HandleAsync(command));

        Assert.Equal("Jogo não encontrado.", result.Message);

        _gameRepositoryMock.Verify(
            x => x.UpdateAsync(It.IsAny<Game>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
}