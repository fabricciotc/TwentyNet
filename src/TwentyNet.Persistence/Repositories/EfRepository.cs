using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using TwentyNet.Domain.Common;
using TwentyNet.Domain.Interfaces;

namespace TwentyNet.Persistence.Repositories;

public sealed class EfRepository<T> : IRepository<T> where T : BaseEntity
{
    private readonly AppDbContext _context;
    private readonly DbSet<T> _set;

    public EfRepository(AppDbContext context)
    {
        _context = context;
        _set = context.Set<T>();
    }

    public Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => _set.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public async Task<IReadOnlyList<T>> ListAsync(Expression<Func<T, bool>>? predicate = null, CancellationToken cancellationToken = default)
    {
        var query = _set.AsNoTracking().AsQueryable();

        if (predicate is not null)
        {
            query = query.Where(predicate);
        }

        return await query.ToListAsync(cancellationToken);
    }

    public async Task<T> AddAsync(T entity, CancellationToken cancellationToken = default)
    {
        await _set.AddAsync(entity, cancellationToken);
        return entity;
    }

    public void Update(T entity)
    {
        var tracked = _context.ChangeTracker.Entries<T>().FirstOrDefault(e => e.Entity.Id == entity.Id);
        if (tracked is not null)
        {
            _context.Entry(tracked.Entity).CurrentValues.SetValues(entity);
        }
        else
        {
            _set.Update(entity);
        }
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _set.FindAsync(new object[] { id }, cancellationToken);
        if (entity is not null)
        {
            _set.Remove(entity);
        }
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => _context.SaveChangesAsync(cancellationToken);
}
