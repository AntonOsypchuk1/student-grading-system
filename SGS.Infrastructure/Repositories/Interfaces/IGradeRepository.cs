using SGS.Domain.Entities;
using SGS.Infrastructure.Repositories.BaseRepository;

namespace SGS.Infrastructure.Repositories.Interfaces;

public interface IGradeRepository : IBaseRepository<Grade>
{
    Task<IEnumerable<Grade>> GetByDocumentIdAsync(Guid documentId, CancellationToken cancellationToken = default);
}