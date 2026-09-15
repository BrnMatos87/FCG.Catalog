using StackExchange.Redis;

namespace FCG.Catalog.Api.Extensions;

public static class RedisServiceExtensions
{
    public static IServiceCollection AddRedisServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString =
            configuration.GetConnectionString("Redis");

        var password =
            configuration["Redis:Password"];

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "A connection string do Redis não foi configurada.");
        }

        services.AddSingleton<IConnectionMultiplexer>(_ =>
        {
            var options =
                ConfigurationOptions.Parse(connectionString);

            options.Password = password;
            options.AbortOnConnectFail = false;

            options.ConnectRetry = 3;
            options.ConnectTimeout = 5000;
            options.SyncTimeout = 5000;

            return ConnectionMultiplexer.Connect(options);
        });

        services.AddScoped<IDatabase>(provider =>
        {
            var connection =
                provider.GetRequiredService<IConnectionMultiplexer>();

            return connection.GetDatabase();
        });

        return services;
    }
}