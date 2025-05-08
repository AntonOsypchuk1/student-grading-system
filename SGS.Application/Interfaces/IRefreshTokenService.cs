using SGS.Application.Models;
using SGS.Domain.Entities;

namespace SGS.Application.Interfaces;

public interface IRefreshTokenService
{
    Task<Result<RefreshToken>> CreateRefreshTokenAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<Result<RefreshToken?>> GetRefreshTokenAsync(string token, CancellationToken cancellationToken = default);
    Task<Result<bool>> RevokeRefreshTokenAsync(string token, CancellationToken cancellationToken = default);
}