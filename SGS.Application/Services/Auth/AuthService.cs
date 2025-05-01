using AutoMapper;
using Microsoft.Extensions.Logging;
using SGS.Application.DTOs;
using SGS.Application.Helpers;
using SGS.Application.Interfaces;
using SGS.Application.Services.Bases;
using SGS.Domain.Entities;
using SGS.Domain.Enums;
using SGS.Infrastructure.Repositories.Interfaces;
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
    public async Task<AuthResponseDto> LoginAsync(string email, string password, CancellationToken cancellationToken = default)
    {
        var user = await UnitOfWork.Users.GetByEmailAsync(email, cancellationToken);
        if (user == null || !PasswordHelper.VerifyPassword(password, user.PasswordHash))
            throw new Exception("Invalid credentials.");

        var accessToken = tokenService.GenerateToken(user);
        var refreshToken = await refreshTokenService.CreateRefreshTokenAsync(user.Id, cancellationToken);
        var userDto = Mapper.Map<UserDto>(user);

        return new AuthResponseDto
        {
            Token = accessToken,
            RefreshToken = refreshToken.Token,
            User = userDto
        };
    }

    public async Task<UserDto> RegisterUserAsync(RegisterUserCommand command, CancellationToken cancellationToken = default)
    {
        return await ExecuteInTransactionAsync(async () =>
        {
            var existingUser = await UnitOfWork.Users.GetByEmailAsync(command.Email, cancellationToken);
            if (existingUser != null)
                throw new Exception("User with this email already exists.");

            if (!Enum.TryParse(command.Role, true, out UserRole role))
                throw new Exception("Invalid role provided.");

            var hashedPassword = PasswordHelper.HashPassword(command.Password);

            var user = new User
            {
                Id = Guid.NewGuid(),
                Login = command.Login,
                FullName = command.FullName,
                Email = command.Email,
                PasswordHash = hashedPassword,
                Role = role
            };

            await UnitOfWork.Users.CreateAsync(user, cancellationToken);
            return Mapper.Map<UserDto>(user);
        }, cancellationToken);
    }
}