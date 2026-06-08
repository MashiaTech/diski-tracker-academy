using System.Linq.Expressions;

namespace DiskiTrack.DataAccess.Repository.Interfaces;

public interface IRepositoryBase<T> where T : class
{
    // Single-entity lookups
    Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);

    // Collection reads
    Task<IReadOnlyList<T>> ListAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<T>> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);

    // IQueryable for LINQ composition (projections, includes, ordering, pagination)
    IQueryable<T> Query();
    IQueryable<T> QueryAsNoTracking();

    // Existence / count
    Task<bool> AnyAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);
    Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null, CancellationToken cancellationToken = default);

    // Writes
    Task AddAsync(T entity, CancellationToken cancellationToken = default);
    Task AddRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default);
    void Update(T entity);
    void Remove(T entity);
    void RemoveRange(IEnumerable<T> entities);

    // Persistence
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}