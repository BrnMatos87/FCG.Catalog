using FCG.Catalog.Application.Abstractions.Queries;
using FCG.Catalog.Application.Contracts;
using FCG.Catalog.Application.Responses;

namespace FCG.Catalog.Application.Queries.Games.Handlers;

public class GetGameByIdQueryHandler : IQueryHandler<GetGameByIdQuery, GameResponse?>
{
    private readonly IGameRepository _gameRepository;

    public GetGameByIdQueryHandler(IGameRepository gameRepository)
    {
        _gameRepository = gameRepository;
    }

    public async Task<GameResponse?> HandleAsync(GetGameByIdQuery query, CancellationToken ct = default)
    {
        var game = await _gameRepository.GetByIdAsync(query.Id, ct);

        if (game is null)
            return null;

        return new GameResponse
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
    }
}