using SGS.DbContext.Repositories.BaseRepository;
using SGS.Domain.Entities;

namespace SGS.DbContext.Repositories.Interfaces;

public interface IGradingCriteriaRepository : IBaseRepository<GradingCriteria>
{
    Task<IEnumerable<GradingCriteria>> GetByAssignmentIdAsync(Guid assignmentId,
        CancellationToken cancellationToken = default);

    Task AddCriteriaAsync(GradingCriteria criteria,
        CancellationToken cancellationToken = default);

    Task AddCriteriaBatchAsync(IEnumerable<GradingCriteria> criteriaList,
        CancellationToken cancellationToken = default);
    
    Task UpdateCriteriaBatchAsync(IEnumerable<GradingCriteria> criteriaList,
        CancellationToken cancellationToken = default);
}