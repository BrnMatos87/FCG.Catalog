using FCG.Catalog.Application.Abstractions.Queries;
using FCG.Catalog.Application.Contracts;
using FCG.Catalog.Application.Queries.Games;
using FCG.Catalog.Application.Responses;

public class GetGamesQueryHandler
    : IQueryHandler<GetGamesQuery, IList<GameResponse>>
{
    private readonly IGameRepository _gameRepository;
    private readonly IGameCache _gameCache;

    public GetGamesQueryHandler(
        IGameRepository gameRepository,
        IGameCache gameCache)
    {
        _gameRepository = gameRepository;
        _gameCache = gameCache;
    }

    public async Task<IList<GameResponse>> HandleAsync(
        GetGamesQuery query,
        CancellationToken ct = default)
    {
        var cachedGames =
            await _gameCache.GetAllAsync(ct);

        if (cachedGames is not null)
        {
            Console.WriteLine($"CACHE HIT - Games");
            return cachedGames;
        }

        var games =
            await _gameRepository.GetAllAsync(ct);

        var response = games
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

        await _gameCache.SetAllAsync(
            response,
            TimeSpan.FromMinutes(5),
            ct);

        return response;
    }
}