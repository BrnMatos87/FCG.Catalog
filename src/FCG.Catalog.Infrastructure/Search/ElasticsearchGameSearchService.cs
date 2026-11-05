using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.QueryDsl;
using FCG.Catalog.Application.Contracts;
using FCG.Catalog.Application.Responses;
using FCG.Catalog.Domain.Entities;
using Microsoft.Extensions.Options;

namespace FCG.Catalog.Infrastructure.Search;

public sealed class ElasticsearchGameSearchService : IGameSearchService
{
    private readonly ElasticsearchClient _client;
    private readonly ElasticsearchOptions _options;

    public ElasticsearchGameSearchService(
        ElasticsearchClient client,
        IOptions<ElasticsearchOptions> options)
    {
        _client = client;
        _options = options.Value;
    }

    public async Task IndexAsync(Game game, CancellationToken ct = default)
    {
        if (!game.IsActive())
        {
            var deleteResponse = await _client.DeleteAsync(
                _options.IndexName,
                game.Id.ToString(),
                ct);

            if (!deleteResponse.IsValidResponse && deleteResponse.ApiCallDetails.HttpStatusCode != 404)
            {
                throw new InvalidOperationException(
                    $"Não foi possível remover o jogo {game.Id} do índice de busca.");
            }

            return;
        }

        var document = new GameSearchDocument
        {
            Id = game.Id,
            Title = game.Title,
            Description = game.Description,
            Price = game.Price,
            Category = game.Category,
            Active = true
        };

        var response = await _client.IndexAsync(
            document,
            _options.IndexName,
            game.Id.ToString(),
            ct);

        if (!response.IsValidResponse)
        {
            throw new InvalidOperationException(
                $"Não foi possível indexar o jogo {game.Id} para busca.");
        }
    }

    public async Task<IReadOnlyList<GameSearchResult>> SearchAsync(
        string term,
        int page,
        int pageSize,
        CancellationToken ct = default)
    {
        var request = new SearchRequest<GameSearchDocument>(_options.IndexName)
        {
            From = (page - 1) * pageSize,
            Size = pageSize,
            Query = new MultiMatchQuery
            {
                Query = term,
                Fields = new[] { "title^3", "category^2", "description" },
                Fuzziness = "AUTO",
                FuzzyTranspositions = true
            }
        };

        var response = await _client.SearchAsync<GameSearchDocument>(request, ct);

        if (!response.IsValidResponse)
            throw new InvalidOperationException("Não foi possível realizar a busca de jogos.");

        return response.Hits
            .Where(hit => hit.Source is not null)
            .Select(hit => new GameSearchResult
            {
                Id = hit.Source!.Id,
                Title = hit.Source.Title,
                Description = hit.Source.Description,
                Price = hit.Source.Price,
                Category = hit.Source.Category,
                Relevance = hit.Score ?? 0
            })
            .ToList();
    }
}
