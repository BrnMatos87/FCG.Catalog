using FCG.Catalog.Application.Abstractions.Commands;
using FCG.Catalog.Application.Contracts;

namespace FCG.Catalog.Application.Commands.Games.Handlers;

public class UpdateGameCommandHandler : ICommandHandlerVoid<UpdateGameCommand>
{
    private readonly IGameRepository _gameRepository;
    private readonly IGameCache _gameCache;
    private readonly IGameSearchService _gameSearchService;

    public UpdateGameCommandHandler(
        IGameRepository gameRepository,
        IGameCache gameCache,
        IGameSearchService gameSearchService)
    {
        _gameRepository = gameRepository;
        _gameCache = gameCache;
        _gameSearchService = gameSearchService;
    }

    public async Task HandleAsync(UpdateGameCommand command, CancellationToken ct = default)
    {
        var game = await _gameRepository.GetByIdAsync(command.Id, ct);

        if (game is null)
            throw new InvalidOperationException("Jogo não encontrado.");

        game.Update(
            command.Title,
            command.Description,
            command.Price,
            command.Category);

        await _gameRepository.UpdateAsync(game, ct);
        await _gameCache.RemoveByIdAsync(game.Id, ct);
        await _gameCache.RemoveAllAsync(ct);
        await _gameSearchService.IndexAsync(game, ct);
    }
}
