using SGS.Domain.Entities;
using SGS.Infrastructure.Repositories.BaseRepository;

namespace SGS.Infrastructure.Repositories.Interfaces;

public interface IAnalysisResultsRepository : IBaseRepository<AnalysisResults>
{
    Task<IEnumerable<AnalysisResults>?> GetByDocumentIdAsync(Guid documentId,
        CancellationToken cancellationToken = default);
}