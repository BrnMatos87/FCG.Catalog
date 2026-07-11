using FCG.Catalog.Application.Contracts;
using FCG.Catalog.Infrastructure.Messaging;
using FCG.Catalog.Infrastructure.Persistence;
using FCG.Catalog.Infrastructure.Repositories;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FCG.Catalog.Infrastructure.Extensions;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<CatalogDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection")));

        services.Configure<RabbitMqOptions>(
            configuration.GetSection("RabbitMq"));

        services.AddScoped<IGameRepository, GameRepository>();
        services.AddScoped<IGameLibraryRepository, GameLibraryRepository>();
        services.AddScoped<ICatalogEventPublisher, MassTransitCatalogEventPublisher>();

        services.AddMassTransit(x =>
        {
            x.UsingRabbitMq((context, cfg) =>
            {
                var options = configuration
                    .GetSection("RabbitMq")
                    .Get<RabbitMqOptions>()!;

                cfg.Host(
                    options.Host,
                    options.Port,
                    options.VirtualHost,
                    h =>
                    {
                        h.Username(options.Username);
                        h.Password(options.Password);
                    });
            });
        });

        return services;
    }
}