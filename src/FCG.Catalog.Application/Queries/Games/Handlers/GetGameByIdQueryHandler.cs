using FCG.Catalog.Application.Abstractions.Queries;
using FCG.Catalog.Application.Contracts;
using FCG.Catalog.Application.Responses;
using Microsoft.Extensions.Logging;

namespace FCG.Catalog.Application.Queries.Games.Handlers;

public class GetGameByIdQueryHandler
    : IQueryHandler<GetGameByIdQuery, GameResponse?>
{
    private readonly IGameRepository _gameRepository;
    private readonly IGameCache _gameCache;
    private readonly ILogger<GetGameByIdQueryHandler> _logger;

    public GetGameByIdQueryHandler(
        IGameRepository gameRepository,
        IGameCache gameCache,
        ILogger<GetGameByIdQueryHandler> logger)
    {
        _gameRepository = gameRepository;
        _gameCache = gameCache;
        _logger = logger;
    }

    public async Task<GameResponse?> HandleAsync(
        GetGameByIdQuery query,
        CancellationToken ct = default)
    {
        var cachedGame =
            await _gameCache.GetByIdAsync(query.Id, ct);

        if (cachedGame is not null)
        {
            _logger.LogInformation(
                "Consulta do jogo atendida pelo Redis. CacheStatus: {CacheStatus}, GameId: {GameId}, CacheKey: {CacheKey}",
                "HIT",
                query.Id,
                $"catalog:game:{query.Id}");

            return cachedGame;
        }

        _logger.LogInformation(
            "Jogo não encontrado no Redis. CacheStatus: {CacheStatus}, GameId: {GameId}, CacheKey: {CacheKey}. Executando consulta normal no SQL Server.",
            "MISS",
            query.Id,
            $"catalog:game:{query.Id}");

        var game =
            await _gameRepository.GetByIdAsync(query.Id, ct);

        if (game is null)
        {
            _logger.LogInformation(
                "Consulta normal no SQL Server concluída sem resultado. GameId: {GameId}",
                query.Id);

            return null;
        }

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

        _logger.LogInformation(
            "Resultado da consulta normal armazenado no Redis. GameId: {GameId}, CacheKey: {CacheKey}, ExpiracaoMinutos: {ExpiracaoMinutos}",
            query.Id,
            $"catalog:game:{query.Id}",
            5);

        return response;
    }
}
