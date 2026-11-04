using FCG.Catalog.Application.Abstractions.Commands;
using FCG.Catalog.Application.Contracts;
using FCG.Catalog.Domain.Entities;

namespace FCG.Catalog.Application.Commands.Games.Handlers;

public class CreateGameCommandHandler : ICommandHandler<CreateGameCommand, Guid>
{
    private readonly IGameRepository _gameRepository;
    private readonly IGameCache _gameCache;
    private readonly IGameSearchService _gameSearchService;

    public CreateGameCommandHandler(
        IGameRepository gameRepository,
        IGameCache gameCache,
        IGameSearchService gameSearchService)
    {
        _gameRepository = gameRepository;
        _gameCache = gameCache;
        _gameSearchService = gameSearchService;
    }

    public async Task<Guid> HandleAsync(CreateGameCommand command, CancellationToken ct = default)
    {
        var existingGame = await _gameRepository.GetByTitleAsync(command.Title, ct);

        if (existingGame is not null)
            throw new InvalidOperationException("Já existe um jogo cadastrado com este título.");

        var game = Game.Create(
            command.Title,
            command.Description,
            command.Price,
            command.Category);

        await _gameRepository.CreateAsync(game, ct);
        await _gameCache.RemoveAllAsync(ct);
        await _gameSearchService.IndexAsync(game, ct);

        return game.Id;
    }
}
