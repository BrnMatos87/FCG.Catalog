using FCG.BuildingBlocks.Events;
using FCG.Catalog.Application.Abstractions.Commands;
using FCG.Catalog.Application.Commands.Purchases;
using MassTransit;

namespace FCG.Catalog.Worker.Consumers;

public class PaymentProcessedConsumer : IConsumer<PaymentProcessedEvent>
{
    private readonly ICommandHandlerVoid<ProcessPaymentCommand> _handler;
    private readonly ILogger<PaymentProcessedConsumer> _logger;

    public PaymentProcessedConsumer(
        ICommandHandlerVoid<ProcessPaymentCommand> handler,
        ILogger<PaymentProcessedConsumer> logger)
    {
        _handler = handler;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<PaymentProcessedEvent> context)
    {
        var message = context.Message;

        _logger.LogInformation(
            "Evento PaymentProcessedEvent recebido. OrderId: {OrderId}, Status: {Status}, CorrelationId: {CorrelationId}",
            message.OrderId,
            message.Status,
            message.CorrelationId);

        await _handler.HandleAsync(new ProcessPaymentCommand
        {
            OrderId = message.OrderId,
            UserId = message.UserId,
            GameId = message.GameId,
            UserEmail = message.UserEmail,
            GameTitle = message.GameTitle,
            Price = message.Price,
            Status = message.Status,
            CorrelationId = message.CorrelationId
        }, context.CancellationToken);

        _logger.LogInformation(
            "Evento PaymentProcessedEvent processado com sucesso. OrderId: {OrderId}, CorrelationId: {CorrelationId}",
            message.OrderId,
            message.CorrelationId);
    }
}