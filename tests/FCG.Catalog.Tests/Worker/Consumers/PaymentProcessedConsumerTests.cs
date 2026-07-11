using FCG.BuildingBlocks.Enums;
using FCG.BuildingBlocks.Events;
using FCG.Catalog.Application.Abstractions.Commands;
using FCG.Catalog.Application.Commands.Purchases;
using FCG.Catalog.Worker.Consumers;
using MassTransit;
using Microsoft.Extensions.Logging;
using Moq;

namespace FCG.Catalog.Tests.Worker.Consumers;

public class PaymentProcessedConsumerTests
{
    private readonly Mock<ICommandHandlerVoid<ProcessPaymentCommand>> _handlerMock;
    private readonly Mock<ILogger<PaymentProcessedConsumer>> _loggerMock;
    private readonly PaymentProcessedConsumer _consumer;

    public PaymentProcessedConsumerTests()
    {
        _handlerMock = new Mock<ICommandHandlerVoid<ProcessPaymentCommand>>();
        _loggerMock = new Mock<ILogger<PaymentProcessedConsumer>>();

        _consumer = new PaymentProcessedConsumer(
            _handlerMock.Object,
            _loggerMock.Object);
    }

    [Fact(DisplayName = "Validando consumo do evento PaymentProcessedEvent")]
    [Trait("Categoria", "Worker - PaymentProcessedConsumer")]
    public async Task Consume_Success()
    {
        var message = new PaymentProcessedEvent
        {
            OrderId = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            GameId = Guid.NewGuid(),
            UserEmail = "usuario@email.com",
            GameTitle = "Sonic",
            Price = 199,
            Status = PaymentStatus.Approved,
            ProcessedAt = DateTime.UtcNow,
            CorrelationId = Guid.NewGuid()
        };

        var contextMock = new Mock<ConsumeContext<PaymentProcessedEvent>>();

        contextMock
            .Setup(x => x.Message)
            .Returns(message);

        contextMock
            .Setup(x => x.CancellationToken)
            .Returns(CancellationToken.None);

        await _consumer.Consume(contextMock.Object);

        _handlerMock.Verify(
            x => x.HandleAsync(
                It.Is<ProcessPaymentCommand>(command =>
                    command.OrderId == message.OrderId &&
                    command.UserId == message.UserId &&
                    command.GameId == message.GameId &&
                    command.UserEmail == message.UserEmail &&
                    command.GameTitle == message.GameTitle &&
                    command.Price == message.Price &&
                    command.Status == message.Status &&
                    command.CorrelationId == message.CorrelationId),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}