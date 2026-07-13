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
using Microsoft.Extensions.Options;

namespace FCG.Catalog.Worker.Extensions;

public static class DependencyInjection
{
    public static IServiceCollection AddWorkerServices(
        this IServiceCollection services,
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
            .Validate(
                options =>
                    !string.IsNullOrWhiteSpace(
                        options.PaymentProcessedQueue),
                "RabbitMq:PaymentProcessedQueue não foi configurado.")
            .ValidateOnStart();

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

        services.AddScoped<IGameRepository, GameRepository>();
        services.AddScoped<IGameLibraryRepository, GameLibraryRepository>();

        services.AddScoped<
            ICommandHandlerVoid<ProcessPaymentCommand>,
            ProcessPaymentCommandHandler>();

        services.AddMassTransit(x =>
        {
            x.AddConsumer<PaymentProcessedConsumer>();

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

                cfg.ReceiveEndpoint(
                    options.PaymentProcessedQueue!,
                    endpoint =>
                    {
                        endpoint.ConfigureConsumer<
                            PaymentProcessedConsumer>(context);
                    });
            });
        });

        return services;
    }
}