using FCG.Catalog.Application.Reviews.Contracts;
using FCG.Catalog.Application.Reviews.Models;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;

namespace FCG.Catalog.Infrastructure.Reviews;

public class MongoGameReviewRepository : IGameReviewRepository
{
    private readonly IMongoCollection<GameReview> _collection;

    public MongoGameReviewRepository(
        IMongoDatabase database,
        IConfiguration configuration)
    {
        var collectionName =
            configuration["MongoDb:ReviewsCollectionName"]
            ?? "reviews";

        _collection =
            database.GetCollection<GameReview>(collectionName);
    }

    public async Task CreateAsync(
        GameReview review,
        CancellationToken ct = default)
    {
        await _collection.InsertOneAsync(
            review,
            cancellationToken: ct);
    }

    public async Task<IReadOnlyList<GameReview>> GetByGameIdAsync(
        Guid gameId,
        CancellationToken ct = default)
    {
        return await _collection
            .Find(x => x.GameId == gameId)
            .SortByDescending(x => x.CreatedAt)
            .ToListAsync(ct);
    }
}