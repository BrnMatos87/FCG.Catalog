using FCG.Catalog.Api.Correlation;
using FCG.Catalog.Application.Abstractions.Commands;
using FCG.Catalog.Application.Abstractions.Queries;
using FCG.Catalog.Application.Commands.Games;
using FCG.Catalog.Application.Commands.Games.Handlers;
using FCG.Catalog.Application.Commands.Purchases;
using FCG.Catalog.Application.Commands.Purchases.Handlers;
using FCG.Catalog.Application.Contracts;
using FCG.Catalog.Application.Queries.Games;
using FCG.Catalog.Application.Queries.Games.Handlers;
using FCG.Catalog.Application.Queries.Library;
using FCG.Catalog.Application.Queries.Library.Handlers;
using FCG.Catalog.Application.Responses;
using FluentValidation;

namespace FCG.Catalog.Api.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<ICorrelationIdAccessor, CorrelationIdAccessor>();

        services.AddValidatorsFromAssembly(typeof(CreateGameCommand).Assembly);

        services.AddScoped<ICommandHandler<CreateGameCommand, Guid>, CreateGameCommandHandler>();
        services.AddScoped<ICommandHandlerVoid<UpdateGameCommand>, UpdateGameCommandHandler>();
        services.AddScoped<ICommandHandlerVoid<ActivateGameCommand>, ActivateGameCommandHandler>();
        services.AddScoped<ICommandHandlerVoid<InactivateGameCommand>, InactivateGameCommandHandler>();

        services.AddScoped<ICommandHandler<CreatePurchaseCommand, Guid>, CreatePurchaseCommandHandler>();
        services.AddScoped<ICommandHandlerVoid<ProcessPaymentCommand>, ProcessPaymentCommandHandler>();

        services.AddScoped<IQueryHandler<GetGameByIdQuery, GameResponse?>, GetGameByIdQueryHandler>();
        services.AddScoped<IQueryHandler<GetGamesQuery, IList<GameResponse>>, GetGamesQueryHandler>();
        services.AddScoped<IQueryHandler<GetUserLibraryQuery, IList<GameLibraryResponse>>, GetUserLibraryQueryHandler>();

        return services;
    }
}