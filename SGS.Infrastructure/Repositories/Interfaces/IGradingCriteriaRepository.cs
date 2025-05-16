using SGS.Domain.Entities;
using SGS.Infrastructure.Repositories.BaseRepository;

namespace SGS.Infrastructure.Repositories.Interfaces;

public interface IGradingCriteriaRepository : IBaseRepository<GradingCriteria>
{
    Task<IEnumerable<GradingCriteria>> GetByAssignmentIdAsync(Guid assignmentId, CancellationToken cancellationToken = default);
}