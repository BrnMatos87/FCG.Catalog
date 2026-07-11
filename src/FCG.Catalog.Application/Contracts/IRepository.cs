using FCG.BuildingBlocks.Domain;

namespace FCG.Catalog.Application.Contracts;

public interface IRepository<T> where T : EntityBase
{
    Task<IList<T>> GetAllAsync(CancellationToken ct = default);

    Task<T?> GetByIdAsync(Guid id, CancellationToken ct = default);

    Task CreateAsync(T entity, CancellationToken ct = default);

    Task CreateManyAsync(IEnumerable<T> entities, CancellationToken ct = default);

    Task UpdateAsync(T entity, CancellationToken ct = default);

    Task DeleteAsync(Guid id, CancellationToken ct = default);
}