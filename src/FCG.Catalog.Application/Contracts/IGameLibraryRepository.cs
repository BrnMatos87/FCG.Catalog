using FCG.Catalog.Domain.Entities;

namespace FCG.Catalog.Application.Contracts;

public interface IGameLibraryRepository : IRepository<GameLibrary>
{
    Task<GameLibrary?> GetByOrderIdAsync(Guid orderId, CancellationToken ct = default);

    Task<GameLibrary?> GetByUserIdAndGameIdAsync(
        Guid userId,
        Guid gameId,
        CancellationToken ct = default);

    Task<IList<GameLibrary>> GetByUserIdAsync(Guid userId, CancellationToken ct = default);
}