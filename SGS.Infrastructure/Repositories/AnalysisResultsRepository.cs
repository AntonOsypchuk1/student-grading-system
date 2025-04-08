using Microsoft.EntityFrameworkCore;
using SGS.Domain.Entities;
using SGS.Infrastructure.Data;
using SGS.Infrastructure.Repositories.BaseRepository;
using SGS.Infrastructure.Repositories.Interfaces;

namespace SGS.Infrastructure.Repositories;

public class AnalysisResultsRepository(AppDbContext context)
    : BaseRepository<AnalysisResults>(context), IAnalysisResultsRepository
{
    public async Task<IEnumerable<AnalysisResults>> GetByDocumentIdAsync(Guid documentId,
        CancellationToken cancellationToken = default)
    {
        return await Context.Set<AnalysisResults>()
            .Where(ar => ar.DocumentId == documentId)
            .ToListAsync(cancellationToken);
    }
}