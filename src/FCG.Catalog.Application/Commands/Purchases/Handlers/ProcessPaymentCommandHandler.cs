using FCG.BuildingBlocks.Enums;
using FCG.Catalog.Application.Abstractions.Commands;
using FCG.Catalog.Application.Contracts;

namespace FCG.Catalog.Application.Commands.Purchases.Handlers;

public class ProcessPaymentCommandHandler : ICommandHandlerVoid<ProcessPaymentCommand>
{
    private readonly IGameLibraryRepository _gameLibraryRepository;

    public ProcessPaymentCommandHandler(IGameLibraryRepository gameLibraryRepository)
    {
        _gameLibraryRepository = gameLibraryRepository;
    }

    public async Task HandleAsync(ProcessPaymentCommand command, CancellationToken ct = default)
    {
        var purchase = await _gameLibraryRepository.GetByOrderIdAsync(command.OrderId, ct);

        if (purchase is null)
            throw new InvalidOperationException("Pedido de compra não encontrado.");

        if (command.Status == PaymentStatus.Approved)
        {
            purchase.Approve();
        }
        else if (command.Status == PaymentStatus.Rejected)
        {
            purchase.Reject();
        }
        else
        {
            throw new InvalidOperationException("Status de pagamento inválido.");
        }

        await _gameLibraryRepository.UpdateAsync(purchase, ct);
    }
}