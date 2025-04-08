using SGS.Domain.Common;

namespace SGS.Infrastructure.Repositories.BaseRepository;

public interface IBaseRepository<TEntity> where TEntity : BaseEntity
{
    Task CreateAsync(TEntity entity, CancellationToken cancellationToken = default);

    Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default);

    Task DeleteAsync(TEntity entity, CancellationToken cancellationToken = default);

    Task<TEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<IEnumerable<TEntity>> GetAllAsync(CancellationToken cancellationToken);
}