using Microsoft.EntityFrameworkCore;
using SGS.Domain.Entities;
using SGS.Infrastructure.Data;
using SGS.Infrastructure.Repositories.BaseRepository;
using SGS.Infrastructure.Repositories.Interfaces;

namespace SGS.Infrastructure.Repositories;

public class GradeRepository(AppDbContext context)
    : BaseRepository<Grade>(context), IGradeRepository
{
    public async Task<IEnumerable<Grade>> GetByDocumentIdAsync(Guid documentId,
        CancellationToken cancellationToken = default)
    {
        return await Context.Grades
            .Where(g => g.DocumentId == documentId)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Grade>> GetByAssignmentIdAsync(Guid assignmentId,
        CancellationToken cancellationToken = default)
    {
        return await Context.Grades
            .Where(g => g.AssignmentId == assignmentId)
            .ToListAsync(cancellationToken);
    }
    
    public async Task AddGradeAsync(Grade grade, CancellationToken cancellationToken = default)
    {
        if (grade.DocumentId == Guid.Empty)
            throw new ArgumentException("Grade must be associated with a document");
            
        await CreateAsync(grade, cancellationToken);
    }
}