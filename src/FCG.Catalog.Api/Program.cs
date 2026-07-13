using FCG.Catalog.Api.Extensions;
using FCG.Catalog.Application.Abstractions.Commands;
using FCG.Catalog.Application.Abstractions.Queries;
using FCG.Catalog.Application.Commands.Games;
using FCG.Catalog.Application.Commands.Games.Handlers;
using FCG.Catalog.Application.Commands.Purchases;
using FCG.Catalog.Application.Commands.Purchases.Handlers;
using FCG.Catalog.Application.Queries.Games;
using FCG.Catalog.Application.Queries.Games.Handlers;
using FCG.Catalog.Application.Queries.Library;
using FCG.Catalog.Application.Queries.Library.Handlers;
using FCG.Catalog.Application.Responses;
using FCG.Catalog.Infrastructure.Extensions;
using FCG.Catalog.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var runningInContainer =
    string.Equals(
        Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER"),
        "true",
        StringComparison.OrdinalIgnoreCase);

if (builder.Environment.IsDevelopment() && !runningInContainer)
{
    builder.Configuration.AddJsonFile(
        "appsettings.Local.json",
        optional: true,
        reloadOnChange: true);
}

builder.Services.AddControllers();

builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddSwaggerDocumentation();
builder.Services.AddApplicationServices();

builder.Services.AddScoped<
    ICommandHandler<CreateGameCommand, Guid>,
    CreateGameCommandHandler>();

builder.Services.AddScoped<
    ICommandHandlerVoid<UpdateGameCommand>,
    UpdateGameCommandHandler>();

builder.Services.AddScoped<
    ICommandHandlerVoid<ActivateGameCommand>,
    ActivateGameCommandHandler>();

builder.Services.AddScoped<
    ICommandHandlerVoid<InactivateGameCommand>,
    InactivateGameCommandHandler>();

builder.Services.AddScoped<
    ICommandHandler<CreatePurchaseCommand, Guid>,
    CreatePurchaseCommandHandler>();

builder.Services.AddScoped<
    ICommandHandlerVoid<ProcessPaymentCommand>,
    ProcessPaymentCommandHandler>();

builder.Services.AddScoped<
    IQueryHandler<GetGamesQuery, IList<GameResponse>>,
    GetGamesQueryHandler>();

builder.Services.AddScoped<
    IQueryHandler<GetGameByIdQuery, GameResponse?>,
    GetGameByIdQueryHandler>();

builder.Services.AddScoped<
    IQueryHandler<GetUserLibraryQuery, IList<GameLibraryResponse>>,
    GetUserLibraryQueryHandler>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext =
        scope.ServiceProvider.GetRequiredService<CatalogDbContext>();

    await dbContext.Database.MigrateAsync();
}

app.UseSwagger();
app.UseSwaggerUI();

app.UseApplicationMiddlewares();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

await app.RunAsync();