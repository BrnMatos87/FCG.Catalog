using FCG.Catalog.Application.Abstractions.Commands;
using FCG.Catalog.Application.Commands.Purchases;
using FCG.Catalog.Application.Commands.Purchases.Handlers;
using FCG.Catalog.Application.Contracts;
using FCG.Catalog.Infrastructure.Messaging;
using FCG.Catalog.Infrastructure.Persistence;
using FCG.Catalog.Infrastructure.Repositories;
using FCG.Catalog.Worker.Consumers;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace FCG.Catalog.Worker.Extensions;

public static class DependencyInjection
{
    public static IServiceCollection AddWorkerServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<CatalogDbContext>(options =>
        {
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"));
        });

        services.AddScoped<IGameRepository, GameRepository>();
        services.AddScoped<IGameLibraryRepository, GameLibraryRepository>();

        services.AddScoped<ICommandHandlerVoid<ProcessPaymentCommand>, ProcessPaymentCommandHandler>();

        services.AddMassTransit(x =>
        {
            x.AddConsumer<PaymentProcessedConsumer>();

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

                cfg.ReceiveEndpoint("catalog-payment-processed", endpoint =>
                {
                    endpoint.ConfigureConsumer<PaymentProcessedConsumer>(context);
                });
            });
        });

        return services;
    }
}