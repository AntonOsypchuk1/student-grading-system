using Microsoft.EntityFrameworkCore;
using SGS.Domain.Entities;
using SGS.Infrastructure.Data;
using SGS.Infrastructure.Repositories.BaseRepository;
using SGS.Infrastructure.Repositories.Interfaces;

namespace SGS.Infrastructure.Repositories;

public class DocumentRepository(AppDbContext context)
    : BaseRepository<Document>(context), IDocumentRepository
{
    public async Task<IEnumerable<Document>> GetDocumentsByUserIdAsync(Guid userId,
        CancellationToken cancellationToken = default)
    {
        return await Context.Documents
            .Where(doc => doc.UserId == userId)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Document>> GetDocumentsByAssignmentIdAsync(Guid assignmentId, CancellationToken cancellationToken = default)
    {
        return await Context.Documents
            .Where(d => d.AssignmentId == assignmentId)
            .ToListAsync(cancellationToken);
    }
}