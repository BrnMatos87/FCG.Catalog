using FCG.Catalog.Application.Responses;
using FCG.Catalog.Domain.Entities;

namespace FCG.Catalog.Application.Contracts;

public interface IGameSearchService
{
    Task IndexAsync(Game game, CancellationToken ct = default);

    Task<IReadOnlyList<GameSearchResult>> SearchAsync(
        string term,
        int page,
        int pageSize,
        CancellationToken ct = default);
}
