using FCG.Catalog.Application.Abstractions.Queries;
using FCG.Catalog.Application.Contracts;
using FCG.Catalog.Application.Responses;

namespace FCG.Catalog.Application.Queries.Library.Handlers;

public class GetUserLibraryQueryHandler : IQueryHandler<GetUserLibraryQuery, IList<GameLibraryResponse>>
{
    private readonly IGameRepository _gameRepository;
    private readonly IGameLibraryRepository _gameLibraryRepository;

    public GetUserLibraryQueryHandler(
        IGameRepository gameRepository,
        IGameLibraryRepository gameLibraryRepository)
    {
        _gameRepository = gameRepository;
        _gameLibraryRepository = gameLibraryRepository;
    }

    public async Task<IList<GameLibraryResponse>> HandleAsync(GetUserLibraryQuery query, CancellationToken ct = default)
    {
        var libraryItems = await _gameLibraryRepository.GetByUserIdAsync(query.UserId, ct);

        var responses = new List<GameLibraryResponse>();

        foreach (var item in libraryItems)
        {
            var game = await _gameRepository.GetByIdAsync(item.GameId, ct);

            responses.Add(new GameLibraryResponse
            {
                Id = item.Id,
                OrderId = item.OrderId,
                UserId = item.UserId,
                GameId = item.GameId,
                GameTitle = game?.Title ?? string.Empty,
                Price = item.Price,
                PurchaseStatus = item.PurchaseStatus,
                Status = item.Status,
                PurchasedAt = item.PurchasedAt,
                CreatedAt = item.CreatedAt
            });
        }

        return responses;
    }
}