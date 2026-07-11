using FCG.BuildingBlocks.Events;
using FCG.Catalog.Infrastructure.Messaging;
using MassTransit;
using Moq;

namespace FCG.Catalog.Tests.Infrastructure.Messaging;

public class MassTransitCatalogEventPublisherTests
{
    private readonly Mock<IPublishEndpoint> _publishEndpointMock;
    private readonly MassTransitCatalogEventPublisher _publisher;

    public MassTransitCatalogEventPublisherTests()
    {
        _publishEndpointMock = new Mock<IPublishEndpoint>();
        _publisher = new MassTransitCatalogEventPublisher(_publishEndpointMock.Object);
    }

    [Fact(DisplayName = "Validando publicação do evento OrderPlacedEvent")]
    [Trait("Categoria", "Infrastructure - Messaging")]
    public async Task PublishOrderPlacedAsync_Success()
    {
        var message = new OrderPlacedEvent
        {
            OrderId = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            GameId = Guid.NewGuid(),
            UserEmail = "usuario@email.com",
            GameTitle = "Sonic",
            Price = 199,
            OccurredAt = DateTime.UtcNow,
            CorrelationId = Guid.NewGuid()
        };

        await _publisher.PublishOrderPlacedAsync(message);

        _publishEndpointMock.Verify(
            x => x.Publish(
                It.Is<OrderPlacedEvent>(e =>
                    e.OrderId == message.OrderId &&
                    e.UserId == message.UserId &&
                    e.GameId == message.GameId &&
                    e.UserEmail == message.UserEmail &&
                    e.GameTitle == message.GameTitle &&
                    e.Price == message.Price &&
                    e.CorrelationId == message.CorrelationId),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}