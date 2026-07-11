using FCG.Catalog.Application.Abstractions.Queries;
using FCG.Catalog.Application.Contracts;
using FCG.Catalog.Application.Responses;

namespace FCG.Catalog.Application.Queries.Games.Handlers;

public class GetGamesQueryHandler : IQueryHandler<GetGamesQuery, IList<GameResponse>>
{
    private readonly IGameRepository _gameRepository;

    public GetGamesQueryHandler(IGameRepository gameRepository)
    {
        _gameRepository = gameRepository;
    }

    public async Task<IList<GameResponse>> HandleAsync(GetGamesQuery query, CancellationToken ct = default)
    {
        var games = await _gameRepository.GetAllAsync(ct);

        return games
            .Select(game => new GameResponse
            {
                Id = game.Id,
                Title = game.Title,
                Description = game.Description,
                Price = game.Price,
                Category = game.Category,
                Status = game.Status,
                CreatedAt = game.CreatedAt,
                UpdatedAt = game.UpdatedAt
            })
            .ToList();
    }
}