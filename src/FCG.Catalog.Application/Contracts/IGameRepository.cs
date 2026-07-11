using FCG.Catalog.Domain.Entities;

namespace FCG.Catalog.Application.Contracts;

public interface IGameRepository : IRepository<Game>
{
    Task<Game?> GetByTitleAsync(string title, CancellationToken ct = default);
}