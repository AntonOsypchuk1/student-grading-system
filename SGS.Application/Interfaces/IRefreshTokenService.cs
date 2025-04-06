using SGS.Domain.Entities;

namespace SGS.Application.Interfaces;

public interface IRefreshTokenService
{
    Task<RefreshToken> CreateRefreshTokenAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<RefreshToken?> GetRefreshTokenAsync(string token, CancellationToken cancellationToken = default);
    Task RevokeRefreshTokenAsync(string token, CancellationToken cancellationToken = default);
}