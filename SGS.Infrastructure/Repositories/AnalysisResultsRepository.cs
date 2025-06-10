using Microsoft.EntityFrameworkCore;
using SGS.Domain.Entities;
using SGS.Infrastructure.Data;
using SGS.Infrastructure.Repositories.BaseRepository;
using SGS.Infrastructure.Repositories.Interfaces;

namespace SGS.Infrastructure.Repositories;

public class AnalysisResultsRepository(AppDbContext context)
    : BaseRepository<AnalysisResults>(context), IAnalysisResultsRepository
{
    public override async Task<AnalysisResults?> GetByIdAsync(Guid id,
        CancellationToken cancellationToken = default)
    {
        return await Context.AnalysisResults
            .Include(ar => ar.Grade)
            .FirstOrDefaultAsync(ar => ar.Id == id, cancellationToken);
    }
    
    public async Task<IEnumerable<AnalysisResults>?> GetByDocumentIdAsync(Guid documentId,
        CancellationToken cancellationToken = default)
    {
        return await Context.AnalysisResults
            .Where(ar => ar.DocumentId == documentId)
            .Include(ar => ar.Grade)
            .Include(ar => ar.Criterias)
            .ToListAsync(cancellationToken);
    }
    
    public async Task<IEnumerable<AnalysisResults>?> GetByGradeIdAsync(Guid gradeId, CancellationToken cancellationToken = default)
    {
        return await Context.AnalysisResults
            .Where(ar => ar.GradeId == gradeId)
            .Include(ar => ar.Document)
            .Include(ar => ar.Criterias)
            .ToListAsync(cancellationToken);
    }
    
    public async Task AddAnalysisResultAsync(AnalysisResults analysisResults, CancellationToken cancellationToken = default)
    {
        if (analysisResults.DocumentId == Guid.Empty)
            throw new ArgumentException("Analysis results must be associated with a document");
            
        if (analysisResults.GradeId == Guid.Empty)
            throw new ArgumentException("Analysis results must be associated with a grade");
            
        if (string.IsNullOrEmpty(analysisResults.GeneratedSummary))
            throw new ArgumentException("Analysis results must have a generated summary");
            
        if (analysisResults.CreatedAt == default)
            analysisResults.CreatedAt = DateTime.UtcNow;
            
        await CreateAsync(analysisResults, cancellationToken);
    }
}