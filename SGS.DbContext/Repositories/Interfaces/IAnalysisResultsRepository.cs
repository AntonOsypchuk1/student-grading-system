using SGS.DbContext.Repositories.BaseRepository;
using SGS.Domain.Entities;

namespace SGS.DbContext.Repositories.Interfaces;

public interface IAnalysisResultsRepository : IBaseRepository<AnalysisResults>
{
    Task<IEnumerable<AnalysisResults>?> GetByDocumentIdAsync(Guid documentId,
        CancellationToken cancellationToken = default);

    Task<IEnumerable<AnalysisResults>?> GetByGradeIdAsync(Guid gradeId,
        CancellationToken cancellationToken = default);

    Task AddAnalysisResultAsync(AnalysisResults analysisResults,
        CancellationToken cancellationToken = default);
}