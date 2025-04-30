using SGS.Domain.Common;

namespace SGS.Infrastructure.Repositories.BaseRepository;

public interface IBaseRepository<TEntity> where TEntity : BaseEntity
{
    Task CreateAsync(TEntity entity, CancellationToken cancellationToken = default);

    void Update(TEntity entity);

    void Delete(TEntity entity);

    Task<TEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<IEnumerable<TEntity>> GetAllAsync(CancellationToken cancellationToken);
}