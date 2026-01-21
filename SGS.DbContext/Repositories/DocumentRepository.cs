using Microsoft.EntityFrameworkCore;
using SGS.DbContext.Data;
using SGS.DbContext.Repositories.BaseRepository;
using SGS.DbContext.Repositories.Interfaces;
using SGS.Domain.Entities;

namespace SGS.DbContext.Repositories;

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
            .OrderByDescending(d => d.UploadedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<Document?> GetDocumentByUserIdAndAssignmentIdAsync(Guid studentId, Guid assignmentId,
        CancellationToken cancellationToken = default)
    {
        var user = await Context.Students
            .FirstOrDefaultAsync(s => s.Id == studentId || s.UserId == studentId, cancellationToken);
        if (user == null)
            throw new ArgumentException("User not found", nameof(studentId));
        
        return await Context.Documents
            .OrderByDescending(d => d.UploadedAt)
            .FirstOrDefaultAsync(d => d.UserId == user.UserId && d.AssignmentId == assignmentId, cancellationToken);
    }
    
    public async Task<IEnumerable<Document>> GetDocumentsByUserIdAndAssignmentIdAsync(Guid userId, Guid assignmentId,
        CancellationToken cancellationToken = default)
    {
        return await Context.Documents
            .Where(d => d.UserId == userId && d.AssignmentId == assignmentId)
            // .OrderByDescending(d => d.UploadedAt)
            .ToListAsync(cancellationToken);
    }
    
    public async Task AddDocumentAsync(Document document, CancellationToken cancellationToken = default)
    {
        if (document.UserId == Guid.Empty)
            throw new ArgumentException("Document must be associated with a user");
            
        if (document.AssignmentId == Guid.Empty)
            throw new ArgumentException("Document must be associated with an assignment");
            
        if (document.UploadedAt == default)
            document.UploadedAt = DateTime.UtcNow;
            
        await CreateAsync(document, cancellationToken);
    }
}