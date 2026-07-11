using FCG.Catalog.Application.Abstractions.Commands;
using FCG.Catalog.Application.Contracts;

namespace FCG.Catalog.Application.Commands.Games.Handlers;

public class InactivateGameCommandHandler : ICommandHandlerVoid<InactivateGameCommand>
{
    private readonly IGameRepository _gameRepository;

    public InactivateGameCommandHandler(IGameRepository gameRepository)
    {
        _gameRepository = gameRepository;
    }

    public async Task HandleAsync(InactivateGameCommand command, CancellationToken ct = default)
    {
        var game = await _gameRepository.GetByIdAsync(command.Id, ct);

        if (game is null)
            throw new InvalidOperationException("Jogo não encontrado.");

        game.Inactivate();

        await _gameRepository.UpdateAsync(game, ct);
    }
}