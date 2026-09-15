using FCG.Catalog.Application.Reviews.Models;
using FCG.Catalog.Infrastructure.Reviews;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using Testcontainers.MongoDb;

namespace FCG.Catalog.Tests.Infrastructure.Reviews;

public class MongoGameReviewRepositoryTests : IAsyncLifetime
{
    private readonly MongoDbContainer _mongoContainer;
    private MongoGameReviewRepository _repository = null!;

    public MongoGameReviewRepositoryTests()
    {
        _mongoContainer = new MongoDbBuilder()
            .WithImage("mongo:7.0")
            .Build();
    }

    public async Task InitializeAsync()
    {
        await _mongoContainer.StartAsync();

        var client = new MongoClient(_mongoContainer.GetConnectionString());

        var database = client.GetDatabase("fcg_catalog_tests");

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["MongoDb:ReviewsCollectionName"] = "reviews"
            })
            .Build();

        _repository = new MongoGameReviewRepository(database, configuration);
    }

    public async Task DisposeAsync()
    {
        await _mongoContainer.DisposeAsync();
    }

    [Fact(DisplayName = "Validando criação de review no MongoDB")]
    [Trait("Categoria", "Infrastructure - MongoGameReviewRepository")]
    public async Task CreateAsync_Success()
    {
        var review = new GameReview
        {
            Id = Guid.NewGuid().ToString(),
            GameId = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Rating = 5,
            Comment = "Excelente jogo!",
            CreatedAt = DateTime.UtcNow
        };

        await _repository.CreateAsync(review);

        var reviews = await _repository.GetByGameIdAsync(review.GameId);

        Assert.Single(reviews);
        Assert.Equal(review.Id, reviews[0].Id);
        Assert.Equal(review.GameId, reviews[0].GameId);
        Assert.Equal(review.UserId, reviews[0].UserId);
        Assert.Equal(review.Rating, reviews[0].Rating);
        Assert.Equal(review.Comment, reviews[0].Comment);
    }

    [Fact(DisplayName = "Validando busca de reviews por jogo ordenadas por data de criação")]
    [Trait("Categoria", "Infrastructure - MongoGameReviewRepository")]
    public async Task GetByGameIdAsync_ReturnsOrderedByCreatedAtDescending()
    {
        var gameId = Guid.NewGuid();

        var olderReview = new GameReview
        {
            Id = Guid.NewGuid().ToString(),
            GameId = gameId,
            UserId = Guid.NewGuid(),
            Rating = 3,
            Comment = "Review antiga",
            CreatedAt = DateTime.UtcNow.AddDays(-1)
        };

        var newerReview = new GameReview
        {
            Id = Guid.NewGuid().ToString(),
            GameId = gameId,
            UserId = Guid.NewGuid(),
            Rating = 4,
            Comment = "Review recente",
            CreatedAt = DateTime.UtcNow
        };

        await _repository.CreateAsync(olderReview);
        await _repository.CreateAsync(newerReview);

        var reviews = await _repository.GetByGameIdAsync(gameId);

        Assert.Equal(2, reviews.Count);
        Assert.Equal(newerReview.Id, reviews[0].Id);
        Assert.Equal(olderReview.Id, reviews[1].Id);
    }

    [Fact(DisplayName = "Validando busca de reviews para jogo sem avaliações")]
    [Trait("Categoria", "Infrastructure - MongoGameReviewRepository")]
    public async Task GetByGameIdAsync_NoReviews_ReturnsEmpty()
    {
        var reviews = await _repository.GetByGameIdAsync(Guid.NewGuid());

        Assert.Empty(reviews);
    }
}
