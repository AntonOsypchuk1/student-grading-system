using Microsoft.EntityFrameworkCore;
using SGS.DbContext.Data;
using SGS.DbContext.Repositories.BaseRepository;
using SGS.DbContext.Repositories.Interfaces;
using SGS.Domain.Entities;

namespace SGS.DbContext.Repositories;

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