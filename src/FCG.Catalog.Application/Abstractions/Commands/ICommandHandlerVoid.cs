namespace FCG.Catalog.Application.Abstractions.Commands;

public interface ICommandHandlerVoid<TCommand>
{
    Task HandleAsync(TCommand command, CancellationToken ct = default);
}