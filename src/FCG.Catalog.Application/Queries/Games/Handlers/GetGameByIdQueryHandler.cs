using FCG.Catalog.Application.Abstractions.Queries;
using FCG.Catalog.Application.Contracts;
using FCG.Catalog.Application.Responses;

namespace FCG.Catalog.Application.Queries.Games.Handlers;

public class GetGameByIdQueryHandler
    : IQueryHandler<GetGameByIdQuery, GameResponse?>
{
    private readonly IGameRepository _gameRepository;
    private readonly IGameCache _gameCache;

    public GetGameByIdQueryHandler(
        IGameRepository gameRepository,
        IGameCache gameCache)
    {
        _gameRepository = gameRepository;
        _gameCache = gameCache;
    }

    public async Task<GameResponse?> HandleAsync(
        GetGameByIdQuery query,
        CancellationToken ct = default)
    {
        var cachedGame =
            await _gameCache.GetByIdAsync(query.Id, ct);

        if (cachedGame is not null)
        {
            Console.WriteLine($"CACHE HIT - Game {query.Id}");
            return cachedGame;
        }

        var game =
            await _gameRepository.GetByIdAsync(query.Id, ct);

        if (game is null)
            return null;

        var response = new GameResponse
        {
            Id = game.Id,
            Title = game.Title,
            Description = game.Description,
            Price = game.Price,
            Category = game.Category,
            Status = game.Status,
            CreatedAt = game.CreatedAt,
            UpdatedAt = game.UpdatedAt
        };

        await _gameCache.SetByIdAsync(
            response,
            TimeSpan.FromMinutes(5),
            ct);

        return response;
    }
}