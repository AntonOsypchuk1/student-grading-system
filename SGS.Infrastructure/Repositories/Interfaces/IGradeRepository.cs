using SGS.Domain.Entities;
using SGS.Infrastructure.Repositories.BaseRepository;

namespace SGS.Infrastructure.Repositories.Interfaces;

public interface IGradeRepository : IBaseRepository<Grade>
{
    Task<IEnumerable<Grade>> GetByDocumentIdAsync(Guid documentId,
        CancellationToken cancellationToken = default);
    
    Task<IEnumerable<Grade>> GetByAssignmentIdAsync(Guid assignmentId,
        CancellationToken cancellationToken = default);
    
    Task AddGradeAsync(Grade grade, CancellationToken cancellationToken = default);
}