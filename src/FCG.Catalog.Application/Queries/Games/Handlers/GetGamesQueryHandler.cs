using FCG.Catalog.Application.Abstractions.Queries;
using FCG.Catalog.Application.Contracts;
using FCG.Catalog.Application.Queries.Games;
using FCG.Catalog.Application.Responses;
using Microsoft.Extensions.Logging;

public class GetGamesQueryHandler
    : IQueryHandler<GetGamesQuery, IList<GameResponse>>
{
    private readonly IGameRepository _gameRepository;
    private readonly IGameCache _gameCache;
    private readonly ILogger<GetGamesQueryHandler> _logger;

    public GetGamesQueryHandler(
        IGameRepository gameRepository,
        IGameCache gameCache,
        ILogger<GetGamesQueryHandler> logger)
    {
        _gameRepository = gameRepository;
        _gameCache = gameCache;
        _logger = logger;
    }

    public async Task<IList<GameResponse>> HandleAsync(
        GetGamesQuery query,
        CancellationToken ct = default)
    {
        var cachedGames =
            await _gameCache.GetAllAsync(ct);

        if (cachedGames is not null)
        {
            _logger.LogInformation(
                "Consulta da lista de jogos atendida pelo Redis. CacheStatus: {CacheStatus}, CacheKey: {CacheKey}, Quantidade: {Quantidade}",
                "HIT",
                "catalog:games",
                cachedGames.Count);

            return cachedGames;
        }

        _logger.LogInformation(
            "Lista de jogos não encontrada no Redis. CacheStatus: {CacheStatus}, CacheKey: {CacheKey}. Executando consulta normal no SQL Server.",
            "MISS",
            "catalog:games");

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

        _logger.LogInformation(
            "Resultado da consulta normal armazenado no Redis. CacheKey: {CacheKey}, ExpiracaoMinutos: {ExpiracaoMinutos}, Quantidade: {Quantidade}",
            "catalog:games",
            5,
            response.Count);

        return response;
    }
}
