using System.Linq.Expressions;
using DiskiTrack.DataAccess.Repository.DbContext;
using DiskiTrack.DataAccess.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DiskiTrack.DataAccess.Repository.Repositories;

public class RepositoryBase<T> : IRepositoryBase<T> where T : class
{
    protected readonly AppDbContext Context;

    public RepositoryBase(AppDbContext context) => Context = context;

    public async Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await Context.Set<T>().FindAsync([id], cancellationToken);

    public async Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
        => await Context.Set<T>().FirstOrDefaultAsync(predicate, cancellationToken);

    public async Task<IReadOnlyList<T>> ListAsync(CancellationToken cancellationToken = default)
        => await Context.Set<T>().ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<T>> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
        => await Context.Set<T>().Where(predicate).ToListAsync(cancellationToken);

    public IQueryable<T> Query()
        => Context.Set<T>();

    public IQueryable<T> QueryAsNoTracking()
        => Context.Set<T>().AsNoTracking();

    public async Task<bool> AnyAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
        => await Context.Set<T>().AnyAsync(predicate, cancellationToken);

    public async Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null, CancellationToken cancellationToken = default)
        => predicate is null
            ? await Context.Set<T>().CountAsync(cancellationToken)
            : await Context.Set<T>().CountAsync(predicate, cancellationToken);

    public async Task AddAsync(T entity, CancellationToken cancellationToken = default)
        => await Context.Set<T>().AddAsync(entity, cancellationToken);

    public async Task AddRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default)
        => await Context.Set<T>().AddRangeAsync(entities, cancellationToken);

    public void Update(T entity)
        => Context.Set<T>().Update(entity);

    public void Remove(T entity)
        => Context.Set<T>().Remove(entity);

    public void RemoveRange(IEnumerable<T> entities)
        => Context.Set<T>().RemoveRange(entities);

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => Context.SaveChangesAsync(cancellationToken);
}