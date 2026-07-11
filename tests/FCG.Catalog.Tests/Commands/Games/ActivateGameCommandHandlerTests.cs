using FCG.BuildingBlocks.Enums;
using FCG.Catalog.Application.Commands.Games;
using FCG.Catalog.Application.Commands.Games.Handlers;
using FCG.Catalog.Application.Contracts;
using FCG.Catalog.Domain.Entities;
using Moq;

namespace FCG.Catalog.Tests.Application.Commands.Games;

public class ActivateGameCommandHandlerTests
{
    private readonly Mock<IGameRepository> _gameRepositoryMock;
    private readonly ActivateGameCommandHandler _handler;

    public ActivateGameCommandHandlerTests()
    {
        _gameRepositoryMock = new Mock<IGameRepository>();
        _handler = new ActivateGameCommandHandler(_gameRepositoryMock.Object);
    }

    [Fact(DisplayName = "Validando ativação de jogo com sucesso")]
    [Trait("Categoria", "Application - ActivateGame")]
    public async Task ActivateGame_HandleAsync_Success()
    {
        var game = Game.Create(
            "Sonic",
            "Jogo de aventura",
            199,
            "Aventura");

        game.Inactivate();

        var command = new ActivateGameCommand
        {
            Id = game.Id
        };

        _gameRepositoryMock
            .Setup(x => x.GetByIdAsync(command.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(game);

        await _handler.HandleAsync(command);

        Assert.Equal(StatusType.Active, game.Status);

        _gameRepositoryMock.Verify(
            x => x.UpdateAsync(game, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact(DisplayName = "Validando ativação de jogo inexistente")]
    [Trait("Categoria", "Application - ActivateGame")]
    public async Task ActivateGame_HandleAsync_GameNotFound()
    {
        var command = new ActivateGameCommand
        {
            Id = Guid.NewGuid()
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