using FCG.Catalog.Application.Contracts;
using FCG.Catalog.Application.Responses;
using StackExchange.Redis;
using System.Text.Json;

namespace FCG.Catalog.Infrastructure.Cache;

public class RedisGameCache : IGameCache
{
    private readonly IDatabase _redis;

    private const string AllGamesKey = "catalog:games";

    public RedisGameCache(IDatabase redis)
    {
        _redis = redis;
    }

    public async Task<GameResponse?> GetByIdAsync(
        Guid id,
        CancellationToken ct = default)
    {
        var value = await _redis.StringGetAsync(GetGameKey(id));

        if (!value.HasValue)
            return null;

        return JsonSerializer.Deserialize<GameResponse>(value!);
    }

    public async Task SetByIdAsync(
        GameResponse game,
        TimeSpan expiration,
        CancellationToken ct = default)
    {
        var json = JsonSerializer.Serialize(game);

        await _redis.StringSetAsync(
            GetGameKey(game.Id),
            json,
            expiration);
    }

    public async Task<IList<GameResponse>?> GetAllAsync(
        CancellationToken ct = default)
    {
        var value = await _redis.StringGetAsync(AllGamesKey);

        if (!value.HasValue)
            return null;

        return JsonSerializer.Deserialize<List<GameResponse>>(value!);
    }

    public async Task SetAllAsync(
        IList<GameResponse> games,
        TimeSpan expiration,
        CancellationToken ct = default)
    {
        var json = JsonSerializer.Serialize(games);

        await _redis.StringSetAsync(
            AllGamesKey,
            json,
            expiration);
    }

    public async Task RemoveByIdAsync(
        Guid id,
        CancellationToken ct = default)
    {
        await _redis.KeyDeleteAsync(GetGameKey(id));
    }

    public async Task RemoveAllAsync(
        CancellationToken ct = default)
    {
        await _redis.KeyDeleteAsync(AllGamesKey);
    }

    private static string GetGameKey(Guid id)
        => $"catalog:game:{id}";
}