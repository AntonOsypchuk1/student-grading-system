using AutoMapper;
using Microsoft.Extensions.Logging;
using SGS.Application.DTOs;
using SGS.Application.Interfaces;
using SGS.Application.Models;
using SGS.Application.Services.Auth.Helpers;
using SGS.Domain.Enums;
using SGS.Infrastructure.Repositories.UnitOfWork;

namespace SGS.Application.Services.Auth;

public class AuthService(
    IUnitOfWork unitOfWork,
    IMapper mapper,
    ILogger<AuthService> logger,
    ITokenService tokenService,
    IRefreshTokenService refreshTokenService)
    : BaseService(unitOfWork, logger, mapper), IAuthService
{
    public async Task<Result<AuthResponseDto>> LoginAsync(string email, string password, CancellationToken cancellationToken = default)
    {
        var user = await UnitOfWork.Users.GetByEmailAsync(email, cancellationToken);
        if (user == null || !PasswordHelper.VerifyPassword(password, user.PasswordHash))
            return Result<AuthResponseDto>.Failure("Invalid credentials.");

        var accessToken = tokenService.GenerateToken(user);
        var refreshToken = await refreshTokenService.CreateRefreshTokenAsync(user.Id, cancellationToken);
        if (!refreshToken.IsSuccess || refreshToken.Data == null)
            return Result<AuthResponseDto>.Failure("Failed to create refresh token.");
        
        var userDto = Mapper.Map<UserDto>(user);

        return Result<AuthResponseDto>.Success(new AuthResponseDto
        {
            Token = accessToken,
            RefreshToken = refreshToken.Data.Token,
            User = userDto
        });
    }

    public async Task<Result<UserDto>> RegisterUserAsync(RegisterUserCommand command, CancellationToken cancellationToken = default)
    {
        return await ExecuteInTransactionAsync(async () =>
        {
            var existingUser = await UnitOfWork.Users.GetByEmailAsync(command.Email, cancellationToken);
            if (existingUser != null)
                throw new Exception("User with this email already exists.");

            if (!Enum.TryParse(command.Role, true, out UserRole role))
                throw new Exception("Invalid role provided.");

            var user = new Domain.Entities.User
            {
                Id = Guid.NewGuid(),
                Login = command.Login,
                FullName = command.FullName,
                Email = command.Email,
                PasswordHash = PasswordHelper.HashPassword(command.Password),
                Role = role
            };

            await UnitOfWork.Users.AddUserAsync(user, cancellationToken);
            return Result<UserDto>.Success(Mapper.Map<UserDto>(user));
        }, cancellationToken);
    }
}