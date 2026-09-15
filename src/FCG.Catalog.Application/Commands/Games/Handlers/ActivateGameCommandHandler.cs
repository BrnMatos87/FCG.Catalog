using FCG.Catalog.Application.Abstractions.Commands;
using FCG.Catalog.Application.Contracts;

namespace FCG.Catalog.Application.Commands.Games.Handlers;

public class ActivateGameCommandHandler : ICommandHandlerVoid<ActivateGameCommand>
{
    private readonly IGameRepository _gameRepository;
    private readonly IGameCache _gameCache;

    public ActivateGameCommandHandler(IGameRepository gameRepository, IGameCache gameCache)
    {
        _gameRepository = gameRepository;
        _gameCache = gameCache;
    }

    public async Task HandleAsync(ActivateGameCommand command, CancellationToken ct = default)
    {
        var game = await _gameRepository.GetByIdAsync(command.Id, ct);

        if (game is null)
            throw new InvalidOperationException("Jogo não encontrado.");

        game.Activate();    

        await _gameRepository.UpdateAsync(game, ct);
        await _gameCache.RemoveByIdAsync(game.Id, ct);
        await _gameCache.RemoveAllAsync(ct);
    }
}