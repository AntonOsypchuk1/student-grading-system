using System.Security.Cryptography;
using SGS.Application.Interfaces;
using SGS.Domain.Entities;
using SGS.Infrastructure.Repositories.Interfaces;

namespace SGS.Application.Services;

public class RefreshTokenService(IRefreshTokenRepository refreshTokenRepository) : IRefreshTokenService
{
    public async Task<RefreshToken> CreateRefreshTokenAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var refreshToken = new RefreshToken
        {
            UserId = userId,
            Token = GenerateRandomToken(),
            Created = DateTime.UtcNow,
            Expires = DateTime.UtcNow.AddDays(7) // for example, valid for 7 days
        };

        await refreshTokenRepository.CreateAsync(refreshToken, cancellationToken);
        return refreshToken;
    }

    public async Task<RefreshToken?> GetRefreshTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        return await refreshTokenRepository.GetByTokenAsync(token, cancellationToken);
    }

    public async Task RevokeRefreshTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        var refreshToken = await GetRefreshTokenAsync(token, cancellationToken);
        if (refreshToken is { IsActive: true })
        {
            refreshToken.Revoked = DateTime.UtcNow;
            await refreshTokenRepository.UpdateAsync(refreshToken, cancellationToken);
        }
    }

    private static string GenerateRandomToken()
    {
        var randomNumber = new byte[32];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }
    }
}