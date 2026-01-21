using SGS.DbContext.Repositories.BaseRepository;
using SGS.Domain.Entities;

namespace SGS.DbContext.Repositories.Interfaces;

public interface IRefreshTokenRepository : IBaseRepository<RefreshToken>
{
    Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken cancellationToken = default);

    Task<IEnumerable<RefreshToken>> GetActiveTokensByUserIdAsync(Guid userId,
        CancellationToken cancellationToken = default);

    Task AddRefreshTokenAsync(RefreshToken refreshToken,
        CancellationToken cancellationToken = default);
}