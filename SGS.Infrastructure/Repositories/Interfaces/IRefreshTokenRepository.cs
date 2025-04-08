using SGS.Domain.Entities;
using SGS.Infrastructure.Repositories.BaseRepository;

namespace SGS.Infrastructure.Repositories.Interfaces;

public interface IRefreshTokenRepository : IBaseRepository<RefreshToken>
{
    Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken cancellationToken = default);

    Task<IEnumerable<RefreshToken>> GetActiveTokensByUserIdAsync(Guid userId,
        CancellationToken cancellationToken = default);
}