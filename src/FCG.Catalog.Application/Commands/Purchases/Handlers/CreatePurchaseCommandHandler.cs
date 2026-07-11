using FCG.BuildingBlocks.Events;
using FCG.Catalog.Application.Abstractions.Commands;
using FCG.Catalog.Application.Contracts;
using FCG.Catalog.Domain.Entities;

namespace FCG.Catalog.Application.Commands.Purchases.Handlers;

public class CreatePurchaseCommandHandler : ICommandHandler<CreatePurchaseCommand, Guid>
{
    private readonly IGameRepository _gameRepository;
    private readonly IGameLibraryRepository _gameLibraryRepository;
    private readonly ICatalogEventPublisher _catalogEventPublisher;
    private readonly ICorrelationIdAccessor _correlationIdAccessor;

    public CreatePurchaseCommandHandler(
        IGameRepository gameRepository,
        IGameLibraryRepository gameLibraryRepository,
        ICatalogEventPublisher catalogEventPublisher,
        ICorrelationIdAccessor correlationIdAccessor)
    {
        _gameRepository = gameRepository;
        _gameLibraryRepository = gameLibraryRepository;
        _catalogEventPublisher = catalogEventPublisher;
        _correlationIdAccessor = correlationIdAccessor;
    }

    public async Task<Guid> HandleAsync(CreatePurchaseCommand command, CancellationToken ct = default)
    {
        var game = await _gameRepository.GetByIdAsync(command.GameId, ct);

        if (game is null)
            throw new InvalidOperationException("Jogo não encontrado.");

        if (!game.IsActive())
            throw new InvalidOperationException("Jogo inativo.");

        var existingPurchase = await _gameLibraryRepository.GetByUserIdAndGameIdAsync(
            command.UserId,
            command.GameId,
            ct);

        if (existingPurchase is not null && existingPurchase.IsApproved())
            throw new InvalidOperationException("Usuário já possui este jogo.");

        var purchase = GameLibrary.CreatePendingPurchase(
            command.UserId,
            command.GameId,
            game.Price);

        await _gameLibraryRepository.CreateAsync(purchase, ct);

        var orderPlacedEvent = new OrderPlacedEvent
        {
            OrderId = purchase.OrderId,
            UserId = purchase.UserId,
            GameId = purchase.GameId,
            UserEmail = command.UserEmail,
            GameTitle = game.Title,
            Price = purchase.Price,
            OccurredAt = DateTime.UtcNow,
            CorrelationId = _correlationIdAccessor.Get()
        };

        await _catalogEventPublisher.PublishOrderPlacedAsync(orderPlacedEvent, ct);

        return purchase.OrderId;
    }
}