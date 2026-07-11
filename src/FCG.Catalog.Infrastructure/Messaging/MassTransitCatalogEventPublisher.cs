using FCG.BuildingBlocks.Events;
using FCG.Catalog.Application.Contracts;
using MassTransit;

namespace FCG.Catalog.Infrastructure.Messaging;

public sealed class MassTransitCatalogEventPublisher : ICatalogEventPublisher
{
    private readonly IPublishEndpoint _publishEndpoint;

    public MassTransitCatalogEventPublisher(IPublishEndpoint publishEndpoint)
    {
        _publishEndpoint = publishEndpoint;
    }

    public async Task PublishOrderPlacedAsync(OrderPlacedEvent message, CancellationToken ct = default)
    {
        await _publishEndpoint.Publish(message, ct);
    }
}