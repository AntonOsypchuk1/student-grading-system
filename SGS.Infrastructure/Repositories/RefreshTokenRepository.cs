using Microsoft.EntityFrameworkCore;
using SGS.Domain.Entities;
using SGS.Infrastructure.Data;
using SGS.Infrastructure.Repositories.BaseRepository;
using SGS.Infrastructure.Repositories.Interfaces;

namespace SGS.Infrastructure.Repositories;

public class RefreshTokenRepository(AppDbContext context)
    : BaseRepository<RefreshToken>(context), IRefreshTokenRepository
{
    public async Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        return await Context.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.Token == token, cancellationToken);
    }

    public async Task<IEnumerable<RefreshToken>> GetActiveTokensByUserIdAsync(Guid userId,
        CancellationToken cancellationToken = default)
    {
        return await Context.RefreshTokens
            .Where(rt => rt.UserId == userId && rt.Revoked == null && rt.Expires > DateTime.UtcNow)
            .ToListAsync(cancellationToken);
    }
    
    public async Task AddRefreshTokenAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default)
    {
        if (refreshToken.UserId == Guid.Empty)
            throw new ArgumentException("Refresh token must be associated with a user");
            
        if (string.IsNullOrEmpty(refreshToken.Token))
            throw new ArgumentException("Refresh token must have a token value");
            
        if (refreshToken.Expires == default)
            throw new ArgumentException("Refresh token must have an expiration date");
            
        await CreateAsync(refreshToken, cancellationToken);
    }
}