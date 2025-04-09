using SGS.Domain.Entities;
using SGS.Infrastructure.Repositories.BaseRepository;

namespace SGS.Infrastructure.Repositories.Interfaces;

public interface IDocumentRepository : IBaseRepository<Document>
{
    Task<IEnumerable<Document>> GetDocumentsByUserIdAsync
        (Guid userId, CancellationToken cancellationToken = default);
    
    Task<IEnumerable<Document>> GetDocumentsByAssignmentIdAsync
        (Guid assignmentId, CancellationToken cancellationToken = default);
}