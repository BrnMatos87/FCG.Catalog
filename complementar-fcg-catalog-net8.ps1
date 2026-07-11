# Execute na raiz do repositorio FCG.Catalog
# Projeto alvo: .NET 8

$ErrorActionPreference = "Stop"

function Write-File {
    param(
        [string]$Path,
        [string]$Content
    )

    $dir = Split-Path $Path -Parent
    if ($dir -and !(Test-Path $dir)) {
        New-Item -ItemType Directory -Force -Path $dir | Out-Null
    }

    Set-Content -Path $Path -Value $Content -Encoding UTF8
}

Write-Host "Criando classes do FCG.Catalog..."

# =========================
# Domain
# =========================
Write-File "src/FCG.Catalog.Domain/Enums/GameStatus.cs" @'
namespace FCG.Catalog.Domain.Enums;

public enum GameStatus
{
    Active = 1,
    Inactive = 2
}
'@

Write-File "src/FCG.Catalog.Domain/Enums/PurchaseStatus.cs" @'
namespace FCG.Catalog.Domain.Enums;

public enum PurchaseStatus
{
    Pending = 1,
    Approved = 2,
    Rejected = 3
}
'@

Write-File "src/FCG.Catalog.Domain/Exceptions/GameDomainException.cs" @'
namespace FCG.Catalog.Domain.Exceptions;

public sealed class GameDomainException : Exception
{
    public GameDomainException(string message) : base(message)
    {
    }
}
'@

Write-File "src/FCG.Catalog.Domain/Entities/Game.cs" @'
using FCG.Catalog.Domain.Enums;
using FCG.Catalog.Domain.Exceptions;

namespace FCG.Catalog.Domain.Entities;

public sealed class Game
{
    private Game() { }

    public Game(string title, string description, string category, decimal price)
    {
        Id = Guid.NewGuid();
        Title = NormalizeRequired(title, nameof(title));
        Description = NormalizeRequired(description, nameof(description));
        Category = NormalizeRequired(category, nameof(category));
        SetPrice(price);
        Status = GameStatus.Active;
        CreatedAt = DateTime.UtcNow;
    }

    public Guid Id { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public string Category { get; private set; } = string.Empty;
    public decimal Price { get; private set; }
    public GameStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    public bool IsActive => Status == GameStatus.Active;

    public void Update(string title, string description, string category, decimal price)
    {
        Title = NormalizeRequired(title, nameof(title));
        Description = NormalizeRequired(description, nameof(description));
        Category = NormalizeRequired(category, nameof(category));
        SetPrice(price);
        UpdatedAt = DateTime.UtcNow;
    }

    public void Activate()
    {
        Status = GameStatus.Active;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Inactivate()
    {
        Status = GameStatus.Inactive;
        UpdatedAt = DateTime.UtcNow;
    }

    public void EnsureCanBePurchased()
    {
        if (!IsActive)
            throw new GameDomainException("Nao e possivel comprar um jogo inativo.");
    }

    private void SetPrice(decimal price)
    {
        if (price <= 0)
            throw new GameDomainException("O preco do jogo deve ser maior que zero.");

        Price = price;
    }

    private static string NormalizeRequired(string value, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new GameDomainException($"O campo {fieldName} e obrigatorio.");

        return value.Trim();
    }
}
'@

Write-File "src/FCG.Catalog.Domain/Entities/GameLibrary.cs" @'
using FCG.Catalog.Domain.Enums;
using FCG.Catalog.Domain.Exceptions;

namespace FCG.Catalog.Domain.Entities;

public sealed class GameLibrary
{
    private GameLibrary() { }

    public GameLibrary(Guid userId, Guid gameId, decimal price)
    {
        if (userId == Guid.Empty)
            throw new GameDomainException("Usuario invalido.");

        if (gameId == Guid.Empty)
            throw new GameDomainException("Jogo invalido.");

        if (price <= 0)
            throw new GameDomainException("O preco da compra deve ser maior que zero.");

        Id = Guid.NewGuid();
        OrderId = Guid.NewGuid();
        UserId = userId;
        GameId = gameId;
        Price = price;
        Status = PurchaseStatus.Pending;
        CreatedAt = DateTime.UtcNow;
    }

    public Guid Id { get; private set; }
    public Guid OrderId { get; private set; }
    public Guid UserId { get; private set; }
    public Guid GameId { get; private set; }
    public decimal Price { get; private set; }
    public PurchaseStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? PurchasedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    public void Approve()
    {
        if (Status == PurchaseStatus.Approved)
            return;

        Status = PurchaseStatus.Approved;
        PurchasedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Reject()
    {
        if (Status == PurchaseStatus.Approved)
            throw new GameDomainException("Nao e possivel rejeitar uma compra ja aprovada.");

        Status = PurchaseStatus.Rejected;
        UpdatedAt = DateTime.UtcNow;
    }
}
'@

# =========================
# Application Contracts and DTOs
# =========================
Write-File "src/FCG.Catalog.Application/Contracts/IGameRepository.cs" @'
using FCG.Catalog.Domain.Entities;

namespace FCG.Catalog.Application.Contracts;

public interface IGameRepository
{
    Task AddAsync(Game game, CancellationToken ct = default);
    Task<Game?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<Game>> GetAllAsync(CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}
'@

Write-File "src/FCG.Catalog.Application/Contracts/IGameLibraryRepository.cs" @'
using FCG.Catalog.Domain.Entities;

namespace FCG.Catalog.Application.Contracts;

public interface IGameLibraryRepository
{
    Task AddAsync(GameLibrary item, CancellationToken ct = default);
    Task<GameLibrary?> GetByOrderIdAsync(Guid orderId, CancellationToken ct = default);
    Task<bool> ExistsApprovedAsync(Guid userId, Guid gameId, CancellationToken ct = default);
    Task<IReadOnlyList<GameLibrary>> GetApprovedByUserIdAsync(Guid userId, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}
'@

Write-File "src/FCG.Catalog.Application/Contracts/IEventPublisher.cs" @'
namespace FCG.Catalog.Application.Contracts;

public interface IEventPublisher
{
    Task PublishAsync<TEvent>(TEvent message, CancellationToken ct = default) where TEvent : class;
}
'@

Write-File "src/FCG.Catalog.Application/Responses/GameResponse.cs" @'
namespace FCG.Catalog.Application.Responses;

public sealed record GameResponse(
    Guid Id,
    string Title,
    string Description,
    string Category,
    decimal Price,
    string Status);
'@

Write-File "src/FCG.Catalog.Application/Responses/GameLibraryResponse.cs" @'
namespace FCG.Catalog.Application.Responses;

public sealed record GameLibraryResponse(
    Guid Id,
    Guid OrderId,
    Guid UserId,
    Guid GameId,
    decimal Price,
    string Status,
    DateTime? PurchasedAt);
'@

# =========================
# Commands / Queries
# =========================
Write-File "src/FCG.Catalog.Application/Commands/Games/CreateGame/CreateGameCommand.cs" @'
namespace FCG.Catalog.Application.Commands.Games.CreateGame;

public sealed record CreateGameCommand(string Title, string Description, string Category, decimal Price);
'@

Write-File "src/FCG.Catalog.Application/Commands/Games/CreateGame/CreateGameCommandHandler.cs" @'
using FCG.Catalog.Application.Contracts;
using FCG.Catalog.Application.Responses;
using FCG.Catalog.Domain.Entities;

namespace FCG.Catalog.Application.Commands.Games.CreateGame;

public sealed class CreateGameCommandHandler(IGameRepository repository)
{
    public async Task<GameResponse> HandleAsync(CreateGameCommand command, CancellationToken ct = default)
    {
        var game = new Game(command.Title, command.Description, command.Category, command.Price);

        await repository.AddAsync(game, ct);
        await repository.SaveChangesAsync(ct);

        return new GameResponse(game.Id, game.Title, game.Description, game.Category, game.Price, game.Status.ToString());
    }
}
'@

Write-File "src/FCG.Catalog.Application/Commands/Games/UpdateGame/UpdateGameCommand.cs" @'
namespace FCG.Catalog.Application.Commands.Games.UpdateGame;

public sealed record UpdateGameCommand(Guid Id, string Title, string Description, string Category, decimal Price);
'@

Write-File "src/FCG.Catalog.Application/Commands/Games/UpdateGame/UpdateGameCommandHandler.cs" @'
using FCG.Catalog.Application.Contracts;
using FCG.Catalog.Application.Responses;

namespace FCG.Catalog.Application.Commands.Games.UpdateGame;

public sealed class UpdateGameCommandHandler(IGameRepository repository)
{
    public async Task<GameResponse> HandleAsync(UpdateGameCommand command, CancellationToken ct = default)
    {
        var game = await repository.GetByIdAsync(command.Id, ct)
            ?? throw new InvalidOperationException("Jogo nao encontrado.");

        game.Update(command.Title, command.Description, command.Category, command.Price);

        await repository.SaveChangesAsync(ct);

        return new GameResponse(game.Id, game.Title, game.Description, game.Category, game.Price, game.Status.ToString());
    }
}
'@

Write-File "src/FCG.Catalog.Application/Commands/Games/DeleteGame/DeleteGameCommand.cs" @'
namespace FCG.Catalog.Application.Commands.Games.DeleteGame;

public sealed record DeleteGameCommand(Guid Id);
'@

Write-File "src/FCG.Catalog.Application/Commands/Games/DeleteGame/DeleteGameCommandHandler.cs" @'
using FCG.Catalog.Application.Contracts;

namespace FCG.Catalog.Application.Commands.Games.DeleteGame;

public sealed class DeleteGameCommandHandler(IGameRepository repository)
{
    public async Task HandleAsync(DeleteGameCommand command, CancellationToken ct = default)
    {
        var game = await repository.GetByIdAsync(command.Id, ct)
            ?? throw new InvalidOperationException("Jogo nao encontrado.");

        game.Inactivate();
        await repository.SaveChangesAsync(ct);
    }
}
'@

Write-File "src/FCG.Catalog.Application/Commands/Purchases/CreatePurchase/CreatePurchaseCommand.cs" @'
namespace FCG.Catalog.Application.Commands.Purchases.CreatePurchase;

public sealed record CreatePurchaseCommand(Guid UserId, Guid GameId);
'@

Write-File "src/FCG.Catalog.Application/Commands/Purchases/CreatePurchase/CreatePurchaseCommandHandler.cs" @'
using FCG.Catalog.Application.Contracts;
using FCG.Catalog.Application.Responses;
using FCG.Catalog.Domain.Entities;
using FCG.BuildingBlocks.Messaging.Events;

namespace FCG.Catalog.Application.Commands.Purchases.CreatePurchase;

public sealed class CreatePurchaseCommandHandler(
    IGameRepository gameRepository,
    IGameLibraryRepository libraryRepository,
    IEventPublisher eventPublisher)
{
    public async Task<GameLibraryResponse> HandleAsync(CreatePurchaseCommand command, CancellationToken ct = default)
    {
        var game = await gameRepository.GetByIdAsync(command.GameId, ct)
            ?? throw new InvalidOperationException("Jogo nao encontrado.");

        game.EnsureCanBePurchased();

        var alreadyPurchased = await libraryRepository.ExistsApprovedAsync(command.UserId, command.GameId, ct);
        if (alreadyPurchased)
            throw new InvalidOperationException("O usuario ja possui este jogo na biblioteca.");

        var libraryItem = new GameLibrary(command.UserId, command.GameId, game.Price);

        await libraryRepository.AddAsync(libraryItem, ct);
        await libraryRepository.SaveChangesAsync(ct);

        await eventPublisher.PublishAsync(
            new OrderPlacedEvent(libraryItem.OrderId, libraryItem.UserId, libraryItem.GameId, libraryItem.Price, DateTime.UtcNow),
            ct);

        return new GameLibraryResponse(
            libraryItem.Id,
            libraryItem.OrderId,
            libraryItem.UserId,
            libraryItem.GameId,
            libraryItem.Price,
            libraryItem.Status.ToString(),
            libraryItem.PurchasedAt);
    }
}
'@

Write-File "src/FCG.Catalog.Application/Commands/Purchases/ProcessPayment/ProcessPaymentCommand.cs" @'
namespace FCG.Catalog.Application.Commands.Purchases.ProcessPayment;

public sealed record ProcessPaymentCommand(Guid OrderId, string Status);
'@

Write-File "src/FCG.Catalog.Application/Commands/Purchases/ProcessPayment/ProcessPaymentCommandHandler.cs" @'
using FCG.Catalog.Application.Contracts;

namespace FCG.Catalog.Application.Commands.Purchases.ProcessPayment;

public sealed class ProcessPaymentCommandHandler(IGameLibraryRepository repository)
{
    public async Task HandleAsync(ProcessPaymentCommand command, CancellationToken ct = default)
    {
        var order = await repository.GetByOrderIdAsync(command.OrderId, ct)
            ?? throw new InvalidOperationException("Pedido nao encontrado.");

        if (string.Equals(command.Status, "Approved", StringComparison.OrdinalIgnoreCase))
            order.Approve();
        else
            order.Reject();

        await repository.SaveChangesAsync(ct);
    }
}
'@

Write-File "src/FCG.Catalog.Application/Queries/Games/GetGameById/GetGameByIdQuery.cs" @'
namespace FCG.Catalog.Application.Queries.Games.GetGameById;

public sealed record GetGameByIdQuery(Guid Id);
'@

Write-File "src/FCG.Catalog.Application/Queries/Games/GetGameById/GetGameByIdQueryHandler.cs" @'
using FCG.Catalog.Application.Contracts;
using FCG.Catalog.Application.Responses;

namespace FCG.Catalog.Application.Queries.Games.GetGameById;

public sealed class GetGameByIdQueryHandler(IGameRepository repository)
{
    public async Task<GameResponse?> HandleAsync(GetGameByIdQuery query, CancellationToken ct = default)
    {
        var game = await repository.GetByIdAsync(query.Id, ct);

        return game is null
            ? null
            : new GameResponse(game.Id, game.Title, game.Description, game.Category, game.Price, game.Status.ToString());
    }
}
'@

Write-File "src/FCG.Catalog.Application/Queries/Games/GetGames/GetGamesQuery.cs" @'
namespace FCG.Catalog.Application.Queries.Games.GetGames;

public sealed record GetGamesQuery;
'@

Write-File "src/FCG.Catalog.Application/Queries/Games/GetGames/GetGamesQueryHandler.cs" @'
using FCG.Catalog.Application.Contracts;
using FCG.Catalog.Application.Responses;

namespace FCG.Catalog.Application.Queries.Games.GetGames;

public sealed class GetGamesQueryHandler(IGameRepository repository)
{
    public async Task<IReadOnlyList<GameResponse>> HandleAsync(GetGamesQuery query, CancellationToken ct = default)
    {
        var games = await repository.GetAllAsync(ct);

        return games
            .Select(game => new GameResponse(game.Id, game.Title, game.Description, game.Category, game.Price, game.Status.ToString()))
            .ToList();
    }
}
'@

Write-File "src/FCG.Catalog.Application/Queries/Library/GetUserLibrary/GetUserLibraryQuery.cs" @'
namespace FCG.Catalog.Application.Queries.Library.GetUserLibrary;

public sealed record GetUserLibraryQuery(Guid UserId);
'@

Write-File "src/FCG.Catalog.Application/Queries/Library/GetUserLibrary/GetUserLibraryQueryHandler.cs" @'
using FCG.Catalog.Application.Contracts;
using FCG.Catalog.Application.Responses;

namespace FCG.Catalog.Application.Queries.Library.GetUserLibrary;

public sealed class GetUserLibraryQueryHandler(IGameLibraryRepository repository)
{
    public async Task<IReadOnlyList<GameLibraryResponse>> HandleAsync(GetUserLibraryQuery query, CancellationToken ct = default)
    {
        var items = await repository.GetApprovedByUserIdAsync(query.UserId, ct);

        return items
            .Select(item => new GameLibraryResponse(item.Id, item.OrderId, item.UserId, item.GameId, item.Price, item.Status.ToString(), item.PurchasedAt))
            .ToList();
    }
}
'@

# =========================
# Infrastructure
# =========================
Write-File "src/FCG.Catalog.Infrastructure/Persistence/CatalogDbContext.cs" @'
using FCG.Catalog.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FCG.Catalog.Infrastructure.Persistence;

public sealed class CatalogDbContext(DbContextOptions<CatalogDbContext> options) : DbContext(options)
{
    public DbSet<Game> Games => Set<Game>();
    public DbSet<GameLibrary> GameLibraries => Set<GameLibrary>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CatalogDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
'@

Write-File "src/FCG.Catalog.Infrastructure/Persistence/Configurations/GameConfiguration.cs" @'
using FCG.Catalog.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FCG.Catalog.Infrastructure.Persistence.Configurations;

public sealed class GameConfiguration : IEntityTypeConfiguration<Game>
{
    public void Configure(EntityTypeBuilder<Game> builder)
    {
        builder.ToTable("Games");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Title).HasMaxLength(150).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(1000).IsRequired();
        builder.Property(x => x.Category).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Price).HasColumnType("decimal(18,2)").IsRequired();
        builder.Property(x => x.Status).HasConversion<int>().IsRequired();
        builder.Property(x => x.CreatedAt).IsRequired();

        builder.HasIndex(x => x.Title);
    }
}
'@

Write-File "src/FCG.Catalog.Infrastructure/Persistence/Configurations/GameLibraryConfiguration.cs" @'
using FCG.Catalog.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FCG.Catalog.Infrastructure.Persistence.Configurations;

public sealed class GameLibraryConfiguration : IEntityTypeConfiguration<GameLibrary>
{
    public void Configure(EntityTypeBuilder<GameLibrary> builder)
    {
        builder.ToTable("GameLibraries");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.OrderId).IsRequired();
        builder.Property(x => x.UserId).IsRequired();
        builder.Property(x => x.GameId).IsRequired();
        builder.Property(x => x.Price).HasColumnType("decimal(18,2)").IsRequired();
        builder.Property(x => x.Status).HasConversion<int>().IsRequired();
        builder.Property(x => x.CreatedAt).IsRequired();

        builder.HasIndex(x => x.OrderId).IsUnique();
        builder.HasIndex(x => new { x.UserId, x.GameId });
    }
}
'@

Write-File "src/FCG.Catalog.Infrastructure/Repositories/GameRepository.cs" @'
using FCG.Catalog.Application.Contracts;
using FCG.Catalog.Domain.Entities;
using FCG.Catalog.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FCG.Catalog.Infrastructure.Repositories;

public sealed class GameRepository(CatalogDbContext context) : IGameRepository
{
    public async Task AddAsync(Game game, CancellationToken ct = default) =>
        await context.Games.AddAsync(game, ct);

    public async Task<Game?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        await context.Games.FirstOrDefaultAsync(x => x.Id == id, ct);

    public async Task<IReadOnlyList<Game>> GetAllAsync(CancellationToken ct = default) =>
        await context.Games.AsNoTracking().OrderBy(x => x.Title).ToListAsync(ct);

    public async Task SaveChangesAsync(CancellationToken ct = default) =>
        await context.SaveChangesAsync(ct);
}
'@

Write-File "src/FCG.Catalog.Infrastructure/Repositories/GameLibraryRepository.cs" @'
using FCG.Catalog.Application.Contracts;
using FCG.Catalog.Domain.Entities;
using FCG.Catalog.Domain.Enums;
using FCG.Catalog.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FCG.Catalog.Infrastructure.Repositories;

public sealed class GameLibraryRepository(CatalogDbContext context) : IGameLibraryRepository
{
    public async Task AddAsync(GameLibrary item, CancellationToken ct = default) =>
        await context.GameLibraries.AddAsync(item, ct);

    public async Task<GameLibrary?> GetByOrderIdAsync(Guid orderId, CancellationToken ct = default) =>
        await context.GameLibraries.FirstOrDefaultAsync(x => x.OrderId == orderId, ct);

    public async Task<bool> ExistsApprovedAsync(Guid userId, Guid gameId, CancellationToken ct = default) =>
        await context.GameLibraries.AnyAsync(x => x.UserId == userId && x.GameId == gameId && x.Status == PurchaseStatus.Approved, ct);

    public async Task<IReadOnlyList<GameLibrary>> GetApprovedByUserIdAsync(Guid userId, CancellationToken ct = default) =>
        await context.GameLibraries
            .AsNoTracking()
            .Where(x => x.UserId == userId && x.Status == PurchaseStatus.Approved)
            .OrderByDescending(x => x.PurchasedAt)
            .ToListAsync(ct);

    public async Task SaveChangesAsync(CancellationToken ct = default) =>
        await context.SaveChangesAsync(ct);
}
'@

Write-File "src/FCG.Catalog.Infrastructure/Messaging/MassTransitEventPublisher.cs" @'
using FCG.Catalog.Application.Contracts;
using MassTransit;

namespace FCG.Catalog.Infrastructure.Messaging;

public sealed class MassTransitEventPublisher(IPublishEndpoint publishEndpoint) : IEventPublisher
{
    public async Task PublishAsync<TEvent>(TEvent message, CancellationToken ct = default) where TEvent : class =>
        await publishEndpoint.Publish(message, ct);
}
'@

Write-File "src/FCG.Catalog.Infrastructure/Messaging/Consumers/PaymentProcessedConsumer.cs" @'
using FCG.Catalog.Application.Commands.Purchases.ProcessPayment;
using FCG.BuildingBlocks.Messaging.Events;
using MassTransit;

namespace FCG.Catalog.Infrastructure.Messaging.Consumers;

public sealed class PaymentProcessedConsumer(ProcessPaymentCommandHandler handler) : IConsumer<PaymentProcessedEvent>
{
    public async Task Consume(ConsumeContext<PaymentProcessedEvent> context)
    {
        await handler.HandleAsync(
            new ProcessPaymentCommand(context.Message.OrderId, context.Message.Status),
            context.CancellationToken);
    }
}
'@

Write-File "src/FCG.Catalog.Infrastructure/Extensions/InfrastructureExtensions.cs" @'
using FCG.Catalog.Application.Commands.Games.CreateGame;
using FCG.Catalog.Application.Commands.Games.DeleteGame;
using FCG.Catalog.Application.Commands.Games.UpdateGame;
using FCG.Catalog.Application.Commands.Purchases.CreatePurchase;
using FCG.Catalog.Application.Commands.Purchases.ProcessPayment;
using FCG.Catalog.Application.Contracts;
using FCG.Catalog.Application.Queries.Games.GetGameById;
using FCG.Catalog.Application.Queries.Games.GetGames;
using FCG.Catalog.Application.Queries.Library.GetUserLibrary;
using FCG.Catalog.Infrastructure.Messaging;
using FCG.Catalog.Infrastructure.Messaging.Consumers;
using FCG.Catalog.Infrastructure.Persistence;
using FCG.Catalog.Infrastructure.Repositories;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FCG.Catalog.Infrastructure.Extensions;

public static class InfrastructureExtensions
{
    public static IServiceCollection AddCatalogInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("CatalogDb")
            ?? configuration["ConnectionStrings:CatalogDb"]
            ?? throw new InvalidOperationException("Connection string CatalogDb nao configurada.");

        services.AddDbContext<CatalogDbContext>(options => options.UseSqlServer(connectionString));

        services.AddScoped<IGameRepository, GameRepository>();
        services.AddScoped<IGameLibraryRepository, GameLibraryRepository>();
        services.AddScoped<IEventPublisher, MassTransitEventPublisher>();

        services.AddScoped<CreateGameCommandHandler>();
        services.AddScoped<UpdateGameCommandHandler>();
        services.AddScoped<DeleteGameCommandHandler>();
        services.AddScoped<CreatePurchaseCommandHandler>();
        services.AddScoped<ProcessPaymentCommandHandler>();
        services.AddScoped<GetGameByIdQueryHandler>();
        services.AddScoped<GetGamesQueryHandler>();
        services.AddScoped<GetUserLibraryQueryHandler>();

        services.AddMassTransit(x =>
        {
            x.AddConsumer<PaymentProcessedConsumer>();

            x.UsingRabbitMq((context, cfg) =>
            {
                var host = configuration["RabbitMq:Host"] ?? "localhost";
                var port = ushort.Parse(configuration["RabbitMq:Port"] ?? "5672");
                var virtualHost = configuration["RabbitMq:VirtualHost"] ?? "/";
                var username = configuration["RabbitMq:Username"] ?? "guest";
                var password = configuration["RabbitMq:Password"] ?? "guest";

                cfg.Host(host, port, virtualHost, h =>
                {
                    h.Username(username);
                    h.Password(password);
                });

                cfg.ReceiveEndpoint(configuration["RabbitMq:PaymentProcessedQueue"] ?? "catalog-payment-processed", e =>
                {
                    e.ConfigureConsumer<PaymentProcessedConsumer>(context);
                });
            });
        });

        return services;
    }
}
'@

# =========================
# API
# =========================
Write-File "src/FCG.Catalog.Api/DTOs/Requests/CreateGameRequest.cs" @'
namespace FCG.Catalog.Api.DTOs.Requests;

public sealed record CreateGameRequest(string Title, string Description, string Category, decimal Price);
'@

Write-File "src/FCG.Catalog.Api/DTOs/Requests/UpdateGameRequest.cs" @'
namespace FCG.Catalog.Api.DTOs.Requests;

public sealed record UpdateGameRequest(string Title, string Description, string Category, decimal Price);
'@

Write-File "src/FCG.Catalog.Api/DTOs/Requests/CreatePurchaseRequest.cs" @'
namespace FCG.Catalog.Api.DTOs.Requests;

public sealed record CreatePurchaseRequest(Guid UserId, Guid GameId);
'@

Write-File "src/FCG.Catalog.Api/Controllers/GamesController.cs" @'
using FCG.Catalog.Api.DTOs.Requests;
using FCG.Catalog.Application.Commands.Games.CreateGame;
using FCG.Catalog.Application.Commands.Games.DeleteGame;
using FCG.Catalog.Application.Commands.Games.UpdateGame;
using FCG.Catalog.Application.Queries.Games.GetGameById;
using FCG.Catalog.Application.Queries.Games.GetGames;
using Microsoft.AspNetCore.Mvc;

namespace FCG.Catalog.Api.Controllers;

[ApiController]
[Route("api/games")]
public sealed class GamesController : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateGameRequest request,
        [FromServices] CreateGameCommandHandler handler,
        CancellationToken ct)
    {
        var response = await handler.HandleAsync(new CreateGameCommand(request.Title, request.Description, request.Category, request.Price), ct);
        return CreatedAtAction(nameof(GetById), new { id = response.Id }, response);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromServices] GetGamesQueryHandler handler, CancellationToken ct)
    {
        var response = await handler.HandleAsync(new GetGamesQuery(), ct);
        return Ok(response);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, [FromServices] GetGameByIdQueryHandler handler, CancellationToken ct)
    {
        var response = await handler.HandleAsync(new GetGameByIdQuery(id), ct);
        return response is null ? NotFound() : Ok(response);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateGameRequest request,
        [FromServices] UpdateGameCommandHandler handler,
        CancellationToken ct)
    {
        var response = await handler.HandleAsync(new UpdateGameCommand(id, request.Title, request.Description, request.Category, request.Price), ct);
        return Ok(response);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, [FromServices] DeleteGameCommandHandler handler, CancellationToken ct)
    {
        await handler.HandleAsync(new DeleteGameCommand(id), ct);
        return NoContent();
    }
}
'@

Write-File "src/FCG.Catalog.Api/Controllers/PurchasesController.cs" @'
using FCG.Catalog.Api.DTOs.Requests;
using FCG.Catalog.Application.Commands.Purchases.CreatePurchase;
using FCG.Catalog.Application.Queries.Library.GetUserLibrary;
using Microsoft.AspNetCore.Mvc;

namespace FCG.Catalog.Api.Controllers;

[ApiController]
[Route("api")]
public sealed class PurchasesController : ControllerBase
{
    [HttpPost("purchases")]
    public async Task<IActionResult> Create(
        [FromBody] CreatePurchaseRequest request,
        [FromServices] CreatePurchaseCommandHandler handler,
        CancellationToken ct)
    {
        var response = await handler.HandleAsync(new CreatePurchaseCommand(request.UserId, request.GameId), ct);
        return Accepted(response);
    }

    [HttpGet("users/{userId:guid}/library")]
    public async Task<IActionResult> GetUserLibrary(Guid userId, [FromServices] GetUserLibraryQueryHandler handler, CancellationToken ct)
    {
        var response = await handler.HandleAsync(new GetUserLibraryQuery(userId), ct);
        return Ok(response);
    }
}
'@

Write-File "src/FCG.Catalog.Api/Program.cs" @'
using FCG.Catalog.Infrastructure.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCatalogInfrastructure(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.MapControllers();

app.Run();
'@

# =========================
# Appsettings complement
# =========================
Write-File "src/FCG.Catalog.Api/appsettings.Development.json" @'
{
  "ConnectionStrings": {
    "CatalogDb": "Server=localhost,1436;Database=FCG_Catalog;User Id=sa;Password=YourStrong!Passw0rd;TrustServerCertificate=True"
  },
  "RabbitMq": {
    "Host": "localhost",
    "Port": 5672,
    "VirtualHost": "/",
    "Username": "guest",
    "Password": "guest",
    "PaymentProcessedQueue": "catalog-payment-processed"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  }
}
'@

# =========================
# BuildingBlocks events - cria somente se o projeto compartilhado existir
# =========================
$buildingBlocksEventsPath = "../Shared/FCG.BuildingBlocks/Messaging/Events"
if (Test-Path "../Shared/FCG.BuildingBlocks") {
    New-Item -ItemType Directory -Force -Path $buildingBlocksEventsPath | Out-Null

    Write-File "$buildingBlocksEventsPath/OrderPlacedEvent.cs" @'
namespace FCG.BuildingBlocks.Messaging.Events;

public sealed record OrderPlacedEvent(
    Guid OrderId,
    Guid UserId,
    Guid GameId,
    decimal Price,
    DateTime OccurredAt);
'@

    Write-File "$buildingBlocksEventsPath/PaymentProcessedEvent.cs" @'
namespace FCG.BuildingBlocks.Messaging.Events;

public sealed record PaymentProcessedEvent(
    Guid OrderId,
    Guid UserId,
    Guid GameId,
    decimal Price,
    string Status,
    DateTime OccurredAt);
'@
}
else {
    Write-Host "Aviso: ../Shared/FCG.BuildingBlocks nao encontrado. Crie os eventos OrderPlacedEvent e PaymentProcessedEvent manualmente no BuildingBlocks."
}

# =========================
# Pacotes e migrations
# =========================
Write-Host "Restaurando pacotes..."
dotnet restore

Write-Host "Script finalizado. Proximos comandos sugeridos:"
Write-Host "dotnet ef migrations add InitialCatalog -p src/FCG.Catalog.Infrastructure -s src/FCG.Catalog.Api"
Write-Host "dotnet ef database update -p src/FCG.Catalog.Infrastructure -s src/FCG.Catalog.Api"
Write-Host "dotnet build"
