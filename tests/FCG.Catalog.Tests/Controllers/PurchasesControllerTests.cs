using FCG.Catalog.Api.Controllers;
using FCG.Catalog.Api.DTOs.Requests;
using FCG.Catalog.Application.Abstractions.Commands;
using FCG.Catalog.Application.Abstractions.Queries;
using FCG.Catalog.Application.Commands.Purchases;
using FCG.Catalog.Application.Queries.Library;
using FCG.Catalog.Application.Responses;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace FCG.Catalog.Tests.Api.Controllers;

public class PurchasesControllerTests
{
    [Fact(DisplayName = "Validando criação de pedido de compra")]
    [Trait("Categoria", "API - PurchasesController")]
    public async Task Create_Success()
    {
        var request = new CreatePurchaseRequest
        {
            UserId = Guid.NewGuid(),
            GameId = Guid.NewGuid(),
            UserEmail = "usuario@email.com"
        };

        var orderId = Guid.NewGuid();

        var handlerMock = new Mock<ICommandHandler<CreatePurchaseCommand, Guid>>();

        handlerMock
            .Setup(x => x.HandleAsync(
                It.IsAny<CreatePurchaseCommand>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(orderId);

        var controller = new PurchasesController();

        var result = await controller.Create(
            request,
            handlerMock.Object,
            CancellationToken.None);

        var acceptedResult = Assert.IsType<AcceptedResult>(result);

        Assert.Equal(202, acceptedResult.StatusCode);

        handlerMock.Verify(
            x => x.HandleAsync(
                It.Is<CreatePurchaseCommand>(command =>
                    command.UserId == request.UserId &&
                    command.GameId == request.GameId &&
                    command.UserEmail == request.UserEmail),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact(DisplayName = "Validando busca da biblioteca do usuário")]
    [Trait("Categoria", "API - PurchasesController")]
    public async Task GetUserLibrary_Success()
    {
        var userId = Guid.NewGuid();

        var response = new List<GameLibraryResponse>
        {
            new()
            {
                Id = Guid.NewGuid(),
                OrderId = Guid.NewGuid(),
                UserId = userId,
                GameId = Guid.NewGuid(),
                GameTitle = "Sonic",
                Price = 199
            }
        };

        var handlerMock = new Mock<IQueryHandler<GetUserLibraryQuery, IList<GameLibraryResponse>>>();

        handlerMock
            .Setup(x => x.HandleAsync(
                It.IsAny<GetUserLibraryQuery>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        var controller = new PurchasesController();

        var result = await controller.GetUserLibrary(
            userId,
            handlerMock.Object,
            CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(result);

        Assert.Equal(200, okResult.StatusCode);
        Assert.Equal(response, okResult.Value);

        handlerMock.Verify(
            x => x.HandleAsync(
                It.Is<GetUserLibraryQuery>(query => query.UserId == userId),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}