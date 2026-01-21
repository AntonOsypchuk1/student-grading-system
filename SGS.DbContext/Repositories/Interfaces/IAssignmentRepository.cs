using SGS.DbContext.Repositories.BaseRepository;
using SGS.Domain.Entities;

namespace SGS.DbContext.Repositories.Interfaces;

public interface IAssignmentRepository : IBaseRepository<Assignment>
{
    Task<IEnumerable<Assignment>> GetByGroupIdAndCourseIdAsync(Guid groupId, Guid courseId,
        CancellationToken cancellationToken = default);
    
    Task AddAssignmentAsync(Assignment assignment,
        CancellationToken cancellationToken = default);

    Task<IEnumerable<Assignment>> GetByGroupIdAsync(Guid groupId,
        CancellationToken cancellationToken = default);

    Task<IEnumerable<Assignment>> GetByCourseIdAsync(Guid courseId,
        CancellationToken cancellationToken = default);
}