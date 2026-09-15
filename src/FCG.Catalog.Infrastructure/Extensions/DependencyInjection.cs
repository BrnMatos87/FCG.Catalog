using FCG.Catalog.Application.Contracts;
using FCG.Catalog.Infrastructure.Messaging;
using FCG.Catalog.Infrastructure.Persistence;
using FCG.Catalog.Infrastructure.Repositories;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using FCG.Catalog.Infrastructure.Cache;
using FCG.Catalog.Application.Reviews.Contracts;
using FCG.Catalog.Infrastructure.Reviews;
using MongoDB.Driver;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;

namespace FCG.Catalog.Infrastructure.Extensions;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        AddDatabase(services, configuration);
        AddRabbitMq(services, configuration);
        AddMongoDb(services, configuration);

        services.AddScoped<IGameRepository, GameRepository>();
        services.AddScoped<IGameLibraryRepository, GameLibraryRepository>();
        services.AddScoped<IGameCache, RedisGameCache>();
        services.AddScoped<IGameReviewRepository, MongoGameReviewRepository>();

        services.AddScoped<
            ICatalogEventPublisher,
            MassTransitCatalogEventPublisher>();

        return services;
    }

    private static void AddDatabase(
        IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString =
            configuration.GetConnectionString("DefaultConnection");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "A connection string 'DefaultConnection' não foi configurada.");
        }

        services.AddDbContext<CatalogDbContext>(options =>
        {
            options.UseSqlServer(connectionString);
        });
    }

    private static void AddRabbitMq(
        IServiceCollection services,
        IConfiguration configuration)
    {
        services
            .AddOptions<RabbitMqOptions>()
            .Bind(configuration.GetSection(RabbitMqOptions.SectionName))
            .Validate(
                options => !string.IsNullOrWhiteSpace(options.Host),
                "RabbitMq:Host não foi configurado.")
            .Validate(
                options => options.Port > 0,
                "RabbitMq:Port deve ser maior que zero.")
            .Validate(
                options => !string.IsNullOrWhiteSpace(options.VirtualHost),
                "RabbitMq:VirtualHost não foi configurado.")
            .Validate(
                options => !string.IsNullOrWhiteSpace(options.Username),
                "RabbitMq:Username não foi configurado.")
            .Validate(
                options => !string.IsNullOrWhiteSpace(options.Password),
                "RabbitMq:Password não foi configurado.")
            .ValidateOnStart();

        services.AddMassTransit(x =>
        {
            x.UsingRabbitMq((context, cfg) =>
            {
                var options = context
                    .GetRequiredService<IOptions<RabbitMqOptions>>()
                    .Value;

                cfg.Host(
                    options.Host,
                    options.Port,
                    options.VirtualHost,
                    hostConfiguration =>
                    {
                        hostConfiguration.Username(options.Username);
                        hostConfiguration.Password(options.Password);
                    });
            });
        });
    }

    private static void AddMongoDb(
     IServiceCollection services,
     IConfiguration configuration)
    {
        BsonSerializer.RegisterSerializer(
            new GuidSerializer(GuidRepresentation.Standard));

        var connectionString =
            configuration["MongoDb:ConnectionString"];

        var databaseName =
            configuration["MongoDb:DatabaseName"];

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "MongoDb:ConnectionString não foi configurado.");
        }

        if (string.IsNullOrWhiteSpace(databaseName))
        {
            throw new InvalidOperationException(
                "MongoDb:DatabaseName não foi configurado.");
        }

        services.AddSingleton<IMongoClient>(_ =>
            new MongoClient(connectionString));

        services.AddScoped<IMongoDatabase>(provider =>
        {
            var client =
                provider.GetRequiredService<IMongoClient>();

            return client.GetDatabase(databaseName);
        });
    }
}