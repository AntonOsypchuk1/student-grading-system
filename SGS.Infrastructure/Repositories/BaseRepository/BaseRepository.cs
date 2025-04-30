using Microsoft.EntityFrameworkCore;
using SGS.Domain.Common;
using SGS.Infrastructure.Data;

namespace SGS.Infrastructure.Repositories.BaseRepository;

public class BaseRepository<TEntity>(AppDbContext context) : IBaseRepository<TEntity>
    where TEntity : BaseEntity
{
    protected AppDbContext Context { get; } = context;

    public async Task CreateAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        await Context.Set<TEntity>().AddAsync(entity, cancellationToken);
    }

    public void Update(TEntity entity)
    {
        Context.Set<TEntity>().Update(entity);
    }

    public void Delete(TEntity entity)
    {
        Context.Set<TEntity>().Remove(entity);
    }

    public async Task<TEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await Context.Set<TEntity>().FindAsync([id], cancellationToken);
    }

    public async Task<IEnumerable<TEntity>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await Context.Set<TEntity>().ToListAsync(cancellationToken);
    }
}