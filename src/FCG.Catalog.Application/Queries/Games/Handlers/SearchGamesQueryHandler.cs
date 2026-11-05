using FCG.Catalog.Application.Abstractions.Queries;
using FCG.Catalog.Application.Contracts;
using FCG.Catalog.Application.Responses;

namespace FCG.Catalog.Application.Queries.Games.Handlers;

public sealed class SearchGamesQueryHandler
    : IQueryHandler<SearchGamesQuery, IReadOnlyList<GameSearchResult>>
{
    private readonly IGameSearchService _gameSearchService;

    public SearchGamesQueryHandler(IGameSearchService gameSearchService)
    {
        _gameSearchService = gameSearchService;
    }

    public Task<IReadOnlyList<GameSearchResult>> HandleAsync(
        SearchGamesQuery query,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(query.Term))
            throw new ArgumentException("O termo de busca é obrigatório.");

        if (query.Page < 1)
            throw new ArgumentException("A página deve ser maior que zero.");

        if (query.PageSize is < 1 or > 50)
            throw new ArgumentException("O tamanho da página deve estar entre 1 e 50.");

        return _gameSearchService.SearchAsync(
            query.Term.Trim(),
            query.Page,
            query.PageSize,
            ct);
    }
}
