using SGS.DbContext.Repositories.BaseRepository;
using SGS.Domain.Entities;

namespace SGS.DbContext.Repositories.Interfaces;

public interface IGradeRepository : IBaseRepository<Grade>
{
    Task<IEnumerable<Grade>> GetByDocumentIdAsync(Guid documentId,
        CancellationToken cancellationToken = default);
    
    Task<IEnumerable<Grade>> GetByAssignmentIdAsync(Guid assignmentId,
        CancellationToken cancellationToken = default);
    
    Task AddGradeAsync(Grade grade, CancellationToken cancellationToken = default);
}