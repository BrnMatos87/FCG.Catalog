using FCG.Catalog.Application.Reviews.Models;

namespace FCG.Catalog.Application.Reviews.Contracts;

public interface IGameReviewRepository
{
    Task CreateAsync(
        GameReview review,
        CancellationToken ct = default);

    Task<IReadOnlyList<GameReview>> GetByGameIdAsync(
        Guid gameId,
        CancellationToken ct = default);
}