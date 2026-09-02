using FCG.Catalog.Application.Responses;

namespace FCG.Catalog.Application.Contracts;

public interface IGameCache
{
    Task<GameResponse?> GetByIdAsync(
        Guid id,
        CancellationToken ct = default);

    Task SetByIdAsync(
        GameResponse game,
        TimeSpan expiration,
        CancellationToken ct = default);

    Task<IList<GameResponse>?> GetAllAsync(
        CancellationToken ct = default);

    Task SetAllAsync(
        IList<GameResponse> games,
        TimeSpan expiration,
        CancellationToken ct = default);

    Task RemoveByIdAsync(
        Guid id,
        CancellationToken ct = default);

    Task RemoveAllAsync(
        CancellationToken ct = default);
}