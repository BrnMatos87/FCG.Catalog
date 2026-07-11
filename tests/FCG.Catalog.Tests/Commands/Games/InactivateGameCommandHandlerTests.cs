using FCG.BuildingBlocks.Enums;
using FCG.Catalog.Application.Commands.Games;
using FCG.Catalog.Application.Commands.Games.Handlers;
using FCG.Catalog.Application.Contracts;
using FCG.Catalog.Domain.Entities;
using Moq;

namespace FCG.Catalog.Tests.Application.Commands.Games;

public class InactivateGameCommandHandlerTests
{
    private readonly Mock<IGameRepository> _gameRepositoryMock;
    private readonly InactivateGameCommandHandler _handler;

    public InactivateGameCommandHandlerTests()
    {
        _gameRepositoryMock = new Mock<IGameRepository>();
        _handler = new InactivateGameCommandHandler(_gameRepositoryMock.Object);
    }

    [Fact(DisplayName = "Validando inativação de jogo com sucesso")]
    [Trait("Categoria", "Application - InactivateGame")]
    public async Task InactivateGame_HandleAsync_Success()
    {
        var game = Game.Create(
            "Sonic",
            "Jogo de aventura",
            199,
            "Aventura");

        var command = new InactivateGameCommand
        {
            Id = game.Id
        };

        _gameRepositoryMock
            .Setup(x => x.GetByIdAsync(command.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(game);

        await _handler.HandleAsync(command);

        Assert.Equal(StatusType.Inactive, game.Status);

        _gameRepositoryMock.Verify(
            x => x.UpdateAsync(game, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact(DisplayName = "Validando inativação de jogo inexistente")]
    [Trait("Categoria", "Application - InactivateGame")]
    public async Task InactivateGame_HandleAsync_GameNotFound()
    {
        var command = new InactivateGameCommand
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