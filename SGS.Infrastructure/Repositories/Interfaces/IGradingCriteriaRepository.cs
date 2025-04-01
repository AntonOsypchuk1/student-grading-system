using SGS.Domain.Entities;
using SGS.Infrastructure.Repositories.BaseRepository;

namespace SGS.Infrastructure.Repositories.Interfaces;

public interface IGradingCriteriaRepository : IBaseRepository<GradingCriteria>
{
    Task<IEnumerable<GradingCriteria>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
}