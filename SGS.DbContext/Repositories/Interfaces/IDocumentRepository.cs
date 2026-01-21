using SGS.DbContext.Repositories.BaseRepository;
using SGS.Domain.Entities;

namespace SGS.DbContext.Repositories.Interfaces;

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