using SGS.Domain.Entities;
using SGS.Infrastructure.Repositories.BaseRepository;

namespace SGS.Infrastructure.Repositories.Interfaces;

public interface IDocumentRepository : IBaseRepository<Document>
{
    Task<IEnumerable<Document>> GetDocumentsByUserIdAsync
        (Guid userId, CancellationToken cancellationToken = default);
    
    Task<IEnumerable<Document>> GetDocumentsByAssignmentIdAsync
        (Guid assignmentId, CancellationToken cancellationToken = default);

    Task<IEnumerable<Document>> GetDocumentsByUserIdAndAssignmentIdAsync(Guid userId, Guid assignmentId,
        CancellationToken cancellationToken = default);

    Task<Document?> GetDocumentByUserIdAndAssignmentIdAsync(Guid userId, Guid assignmentId,
        CancellationToken cancellationToken = default);

    Task AddDocumentAsync(Document document,
        CancellationToken cancellationToken = default);
}