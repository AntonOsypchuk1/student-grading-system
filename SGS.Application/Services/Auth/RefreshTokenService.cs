using System.Security.Cryptography;
using SGS.Application.Interfaces;
using SGS.Domain.Entities;
using SGS.Infrastructure.Data;
using SGS.Infrastructure.Repositories.Interfaces;
using SGS.Infrastructure.Repositories.UnitOfWork;

namespace SGS.Application.Services.Auth;

public class RefreshTokenService(IUnitOfWork unitOfWork) : IRefreshTokenService
{
    private readonly IRefreshTokenRepository _refreshTokenRepository = unitOfWork.RefreshTokens;
    
    public async Task<RefreshToken> CreateRefreshTokenAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var refreshToken = new RefreshToken
        {
            UserId = userId,
            Token = GenerateRandomToken(),
            Created = DateTime.UtcNow,
            Expires = DateTime.UtcNow.AddDays(1)
        };

        await _refreshTokenRepository.CreateAsync(refreshToken, cancellationToken);
        return refreshToken;
    }

    public async Task<RefreshToken?> GetRefreshTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        return await _refreshTokenRepository.GetByTokenAsync(token, cancellationToken);
    }

    public async Task RevokeRefreshTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        var refreshToken = await GetRefreshTokenAsync(token, cancellationToken);
        if (refreshToken is { IsActive: true })
        {
            refreshToken.Revoked = DateTime.UtcNow;
            _refreshTokenRepository.Update(refreshToken);
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