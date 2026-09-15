using FCG.Catalog.Api.Controllers;
using FCG.Catalog.Api.DTOs.Requests;
using FCG.Catalog.Application.Reviews.Contracts;
using FCG.Catalog.Application.Reviews.Models;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace FCG.Catalog.Tests.Api.Controllers;

public class GameReviewsControllerTests
{
    private readonly Mock<IGameReviewRepository> _repositoryMock;
    private readonly GameReviewsController _controller;

    public GameReviewsControllerTests()
    {
        _repositoryMock = new Mock<IGameReviewRepository>();
        _controller = new GameReviewsController(_repositoryMock.Object);
    }

    [Fact(DisplayName = "Validando criação de review com sucesso")]
    [Trait("Categoria", "API - GameReviewsController")]
    public async Task CreateAsync_Success()
    {
        var gameId = Guid.NewGuid();

        var request = new CreateGameReviewRequest
        {
            UserId = Guid.NewGuid(),
            Rating = 5,
            Comment = "Muito bom!"
        };

        var result = await _controller.CreateAsync(
            gameId,
            request,
            CancellationToken.None);

        var createdResult = Assert.IsType<CreatedResult>(result);

        Assert.Equal(201, createdResult.StatusCode);

        var review = Assert.IsType<GameReview>(createdResult.Value);

        Assert.Equal(gameId, review.GameId);
        Assert.Equal(request.UserId, review.UserId);
        Assert.Equal(request.Rating, review.Rating);
        Assert.Equal(request.Comment, review.Comment);
        Assert.NotEqual(Guid.Empty.ToString(), review.Id);
        Assert.Equal($"/api/v1/games/{gameId}/reviews/{review.Id}", createdResult.Location);

        _repositoryMock.Verify(
            x => x.CreateAsync(
                It.Is<GameReview>(r =>
                    r.GameId == gameId &&
                    r.UserId == request.UserId &&
                    r.Rating == request.Rating &&
                    r.Comment == request.Comment),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Theory(DisplayName = "Validando criação de review com rating inválido")]
    [Trait("Categoria", "API - GameReviewsController")]
    [InlineData(0)]
    [InlineData(6)]
    public async Task CreateAsync_InvalidRating_ReturnsBadRequest(int rating)
    {
        var gameId = Guid.NewGuid();

        var request = new CreateGameReviewRequest
        {
            UserId = Guid.NewGuid(),
            Rating = rating,
            Comment = "Comentário"
        };

        var result = await _controller.CreateAsync(
            gameId,
            request,
            CancellationToken.None);

        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);

        Assert.Equal(400, badRequestResult.StatusCode);
        Assert.Equal("Rating deve estar entre 1 e 5.", badRequestResult.Value);

        _repositoryMock.Verify(
            x => x.CreateAsync(It.IsAny<GameReview>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact(DisplayName = "Validando busca de reviews por jogo")]
    [Trait("Categoria", "API - GameReviewsController")]
    public async Task GetByGameIdAsync_Success()
    {
        var gameId = Guid.NewGuid();

        var reviews = new List<GameReview>
        {
            new()
            {
                Id = Guid.NewGuid().ToString(),
                GameId = gameId,
                UserId = Guid.NewGuid(),
                Rating = 4,
                Comment = "Bom jogo",
                CreatedAt = DateTime.UtcNow
            }
        };

        _repositoryMock
            .Setup(x => x.GetByGameIdAsync(gameId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(reviews);

        var result = await _controller.GetByGameIdAsync(gameId, CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(result);

        Assert.Equal(200, okResult.StatusCode);
        Assert.Equal(reviews, okResult.Value);

        _repositoryMock.Verify(
            x => x.GetByGameIdAsync(gameId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact(DisplayName = "Validando busca de reviews sem resultados")]
    [Trait("Categoria", "API - GameReviewsController")]
    public async Task GetByGameIdAsync_Empty()
    {
        var gameId = Guid.NewGuid();

        _repositoryMock
            .Setup(x => x.GetByGameIdAsync(gameId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<GameReview>());

        var result = await _controller.GetByGameIdAsync(gameId, CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(result);

        var reviews = Assert.IsAssignableFrom<IReadOnlyList<GameReview>>(okResult.Value);

        Assert.Empty(reviews);
    }
}
