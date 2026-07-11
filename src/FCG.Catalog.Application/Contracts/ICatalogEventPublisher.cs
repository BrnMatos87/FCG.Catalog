using FCG.BuildingBlocks.Events;

namespace FCG.Catalog.Application.Contracts;

public interface ICatalogEventPublisher
{
    Task PublishOrderPlacedAsync(OrderPlacedEvent message, CancellationToken ct = default);
}