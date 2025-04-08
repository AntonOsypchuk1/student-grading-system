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
}