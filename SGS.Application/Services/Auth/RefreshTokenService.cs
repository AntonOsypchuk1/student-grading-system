using System.Security.Cryptography;
using AutoMapper;
using Microsoft.Extensions.Logging;
using SGS.Application.Helpers;
using SGS.Application.Interfaces;
using SGS.Domain.Entities;
using SGS.Infrastructure.Repositories.UnitOfWork;

namespace SGS.Application.Services.Auth;

public class RefreshTokenService(
    IUnitOfWork unitOfWork,
    ILogger<RefreshTokenService> logger,
    IMapper mapper)
    : BaseService(unitOfWork, logger, mapper), IRefreshTokenService
{
    public async Task<Result<RefreshToken>> CreateRefreshTokenAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await ExecuteInTransactionAsync(async () =>
        {
            var user = await UnitOfWork.Users.GetByIdAsync(userId, cancellationToken);
            if (user == null)
                return Result<RefreshToken>.Failure("User not found.");
            
            var refreshToken = new RefreshToken
            {
                UserId = userId,
                Token = GenerateRandomToken(),
                Created = DateTime.UtcNow,
                Expires = DateTime.UtcNow.AddDays(1)
            };

            await UnitOfWork.RefreshTokens.CreateAsync(refreshToken, cancellationToken);
            return Result<RefreshToken>.Success(refreshToken);
        }, cancellationToken);
    }

    public async Task<Result<RefreshToken?>> GetRefreshTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        try
        {
            var refreshToken = await UnitOfWork.RefreshTokens.GetByTokenAsync(token, cancellationToken);
            return Result<RefreshToken?>.Success(refreshToken);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error retrieving refresh token");
            return Result<RefreshToken?>.Failure($"Failed to retrieve refresh token: {ex.Message}");
        }
    }

    public async Task<Result<bool>> RevokeRefreshTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        return await ExecuteInTransactionAsync(async () =>
        {
            var refreshToken = await UnitOfWork.RefreshTokens.GetByTokenAsync(token, cancellationToken);
            if (refreshToken == null)
                return Result<bool>.Failure("Refresh token not found");

            refreshToken.Revoked = DateTime.UtcNow;
            UnitOfWork.RefreshTokens.Update(refreshToken);

            return Result<bool>.Success(true);
        }, cancellationToken);
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