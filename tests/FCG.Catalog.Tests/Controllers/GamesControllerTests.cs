using FCG.Catalog.Api.Controllers;
using FCG.Catalog.Api.DTOs.Requests;
using FCG.Catalog.Application.Abstractions.Commands;
using FCG.Catalog.Application.Abstractions.Queries;
using FCG.Catalog.Application.Commands.Games;
using FCG.Catalog.Application.Queries.Games;
using FCG.Catalog.Application.Responses;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace FCG.Catalog.Tests.Api.Controllers;

public class GamesControllerTests
{
    [Fact(DisplayName = "Validando criação de jogo")]
    [Trait("Categoria", "API - GamesController")]
    public async Task Create_Success()
    {
        var request = new CreateGameRequest
        {
            Title = "Sonic",
            Description = "Jogo de aventura",
            Price = 199,
            Category = "Aventura"
        };

        var gameId = Guid.NewGuid();

        var handlerMock = new Mock<ICommandHandler<CreateGameCommand, Guid>>();

        handlerMock
            .Setup(x => x.HandleAsync(
                It.IsAny<CreateGameCommand>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(gameId);

        var controller = new GamesController();

        var result = await controller.Create(
            request,
            handlerMock.Object,
            CancellationToken.None);

        var createdResult = Assert.IsType<CreatedAtActionResult>(result);

        Assert.Equal(nameof(GamesController.GetById), createdResult.ActionName);
        Assert.Equal(201, createdResult.StatusCode);

        handlerMock.Verify(
            x => x.HandleAsync(
                It.Is<CreateGameCommand>(command =>
                    command.Title == request.Title &&
                    command.Description == request.Description &&
                    command.Price == request.Price &&
                    command.Category == request.Category),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact(DisplayName = "Validando busca de todos os jogos")]
    [Trait("Categoria", "API - GamesController")]
    public async Task GetAll_Success()
    {
        var response = new List<GameResponse>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Title = "Sonic",
                Description = "Jogo de aventura",
                Price = 199,
                Category = "Aventura"
            }
        };

        var handlerMock = new Mock<IQueryHandler<GetGamesQuery, IList<GameResponse>>>();

        handlerMock
            .Setup(x => x.HandleAsync(
                It.IsAny<GetGamesQuery>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        var controller = new GamesController();

        var result = await controller.GetAll(
            handlerMock.Object,
            CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(result);

        Assert.Equal(200, okResult.StatusCode);
        Assert.Equal(response, okResult.Value);

        handlerMock.Verify(
            x => x.HandleAsync(
                It.IsAny<GetGamesQuery>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact(DisplayName = "Validando busca de jogo por id")]
    [Trait("Categoria", "API - GamesController")]
    public async Task GetById_Success()
    {
        var gameId = Guid.NewGuid();

        var response = new GameResponse
        {
            Id = gameId,
            Title = "Sonic",
            Description = "Jogo de aventura",
            Price = 199,
            Category = "Aventura"
        };

        var handlerMock = new Mock<IQueryHandler<GetGameByIdQuery, GameResponse?>>();

        handlerMock
            .Setup(x => x.HandleAsync(
                It.IsAny<GetGameByIdQuery>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        var controller = new GamesController();

        var result = await controller.GetById(
            gameId,
            handlerMock.Object,
            CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(result);

        Assert.Equal(200, okResult.StatusCode);
        Assert.Equal(response, okResult.Value);

        handlerMock.Verify(
            x => x.HandleAsync(
                It.Is<GetGameByIdQuery>(query => query.Id == gameId),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact(DisplayName = "Validando busca de jogo inexistente")]
    [Trait("Categoria", "API - GamesController")]
    public async Task GetById_NotFound()
    {
        var gameId = Guid.NewGuid();

        var handlerMock = new Mock<IQueryHandler<GetGameByIdQuery, GameResponse?>>();

        handlerMock
            .Setup(x => x.HandleAsync(
                It.IsAny<GetGameByIdQuery>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((GameResponse?)null);

        var controller = new GamesController();

        var result = await controller.GetById(
            gameId,
            handlerMock.Object,
            CancellationToken.None);

        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);

        Assert.Equal(404, notFoundResult.StatusCode);
    }

    [Fact(DisplayName = "Validando atualização de jogo")]
    [Trait("Categoria", "API - GamesController")]
    public async Task Update_Success()
    {
        var gameId = Guid.NewGuid();

        var request = new UpdateGameRequest
        {
            Title = "Sonic 2",
            Description = "Novo jogo",
            Price = 250,
            Category = "Plataforma"
        };

        var handlerMock = new Mock<ICommandHandlerVoid<UpdateGameCommand>>();

        var controller = new GamesController();

        var result = await controller.Update(
            gameId,
            request,
            handlerMock.Object,
            CancellationToken.None);

        Assert.IsType<NoContentResult>(result);

        handlerMock.Verify(
            x => x.HandleAsync(
                It.Is<UpdateGameCommand>(command =>
                    command.Id == gameId &&
                    command.Title == request.Title &&
                    command.Description == request.Description &&
                    command.Price == request.Price &&
                    command.Category == request.Category),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact(DisplayName = "Validando ativação de jogo")]
    [Trait("Categoria", "API - GamesController")]
    public async Task Activate_Success()
    {
        var gameId = Guid.NewGuid();

        var handlerMock = new Mock<ICommandHandlerVoid<ActivateGameCommand>>();

        var controller = new GamesController();

        var result = await controller.Activate(
            gameId,
            handlerMock.Object,
            CancellationToken.None);

        Assert.IsType<NoContentResult>(result);

        handlerMock.Verify(
            x => x.HandleAsync(
                It.Is<ActivateGameCommand>(command => command.Id == gameId),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact(DisplayName = "Validando inativação de jogo")]
    [Trait("Categoria", "API - GamesController")]
    public async Task Inactivate_Success()
    {
        var gameId = Guid.NewGuid();

        var handlerMock = new Mock<ICommandHandlerVoid<InactivateGameCommand>>();

        var controller = new GamesController();

        var result = await controller.Inactivate(
            gameId,
            handlerMock.Object,
            CancellationToken.None);

        Assert.IsType<NoContentResult>(result);

        handlerMock.Verify(
            x => x.HandleAsync(
                It.Is<InactivateGameCommand>(command => command.Id == gameId),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}