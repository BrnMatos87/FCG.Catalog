using FCG.BuildingBlocks.Enums;
using FCG.Catalog.Application.Contracts;
using FCG.Catalog.Domain.Entities;
using FCG.Catalog.Domain.Enums;
using FCG.Catalog.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FCG.Catalog.Infrastructure.Repositories;

public class GameLibraryRepository : IGameLibraryRepository
{
    private readonly CatalogDbContext _context;

    public GameLibraryRepository(CatalogDbContext context)
    {
        _context = context;
    }

    public async Task<IList<GameLibrary>> GetAllAsync(CancellationToken ct = default)
    {
        return await _context.GameLibraries
            .AsNoTracking()
            .Where(x => x.Status == StatusType.Active)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<GameLibrary?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.GameLibraries
            .FirstOrDefaultAsync(x => x.Id == id, ct);
    }

    public async Task<GameLibrary?> GetByOrderIdAsync(Guid orderId, CancellationToken ct = default)
    {
        return await _context.GameLibraries
            .FirstOrDefaultAsync(x => x.OrderId == orderId, ct);
    }

    public async Task<GameLibrary?> GetByUserIdAndGameIdAsync(
        Guid userId,
        Guid gameId,
        CancellationToken ct = default)
    {
        return await _context.GameLibraries
            .FirstOrDefaultAsync(x =>
                x.UserId == userId &&
                x.GameId == gameId &&
                x.Status == StatusType.Active,
                ct);
    }

    public async Task<IList<GameLibrary>> GetByUserIdAsync(Guid userId, CancellationToken ct = default)
    {
        return await _context.GameLibraries
            .AsNoTracking()
            .Where(x =>
                x.UserId == userId &&
                x.Status == StatusType.Active &&
                x.PurchaseStatus == PurchaseStatus.Approved)
            .OrderByDescending(x => x.PurchasedAt)
            .ToListAsync(ct);
    }

    public async Task CreateAsync(GameLibrary entity, CancellationToken ct = default)
    {
        await _context.GameLibraries.AddAsync(entity, ct);
        await _context.SaveChangesAsync(ct);
    }

    public async Task CreateManyAsync(IEnumerable<GameLibrary> entities, CancellationToken ct = default)
    {
        await _context.GameLibraries.AddRangeAsync(entities, ct);
        await _context.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(GameLibrary entity, CancellationToken ct = default)
    {
        _context.GameLibraries.Update(entity);
        await _context.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var libraryItem = await GetByIdAsync(id, ct);

        if (libraryItem is null)
            return;

        libraryItem.Inactivate();

        _context.GameLibraries.Update(libraryItem);
        await _context.SaveChangesAsync(ct);
    }
}