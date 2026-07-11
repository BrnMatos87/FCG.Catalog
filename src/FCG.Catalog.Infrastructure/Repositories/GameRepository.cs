using FCG.BuildingBlocks.Enums;
using FCG.Catalog.Application.Contracts;
using FCG.Catalog.Domain.Entities;
using FCG.Catalog.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FCG.Catalog.Infrastructure.Repositories;

public class GameRepository : IGameRepository
{
    private readonly CatalogDbContext _context;

    public GameRepository(CatalogDbContext context)
    {
        _context = context;
    }

    public async Task<IList<Game>> GetAllAsync(CancellationToken ct = default)
    {
        return await _context.Games
            .AsNoTracking()
            .Where(x => x.Status == StatusType.Active)
            .OrderBy(x => x.Title)
            .ToListAsync(ct);
    }

    public async Task<Game?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.Games
            .FirstOrDefaultAsync(x => x.Id == id, ct);
    }

    public async Task<Game?> GetByTitleAsync(string title, CancellationToken ct = default)
    {
        var normalizedTitle = title.Trim();

        return await _context.Games
            .FirstOrDefaultAsync(x => x.Title == normalizedTitle, ct);
    }

    public async Task CreateAsync(Game entity, CancellationToken ct = default)
    {
        await _context.Games.AddAsync(entity, ct);
        await _context.SaveChangesAsync(ct);
    }

    public async Task CreateManyAsync(IEnumerable<Game> entities, CancellationToken ct = default)
    {
        await _context.Games.AddRangeAsync(entities, ct);
        await _context.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(Game entity, CancellationToken ct = default)
    {
        _context.Games.Update(entity);
        await _context.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var game = await GetByIdAsync(id, ct);

        if (game is null)
            return;

        game.Inactivate();

        _context.Games.Update(game);
        await _context.SaveChangesAsync(ct);
    }
}