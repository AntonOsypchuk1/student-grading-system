using SGS.Domain.Entities;
using SGS.Infrastructure.Repositories.BaseRepository;

namespace SGS.Infrastructure.Repositories.Interfaces;

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