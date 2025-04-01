using SGS.Domain.Entities;
using SGS.Infrastructure.Repositories.BaseRepository;

namespace SGS.Infrastructure.Repositories.Interfaces;

public interface IAssignmentRepository : IBaseRepository<Assignment>
{
    Task<IEnumerable<Assignment>> GetByGroupIdAsync(Guid groupId, CancellationToken cancellationToken = default);
}