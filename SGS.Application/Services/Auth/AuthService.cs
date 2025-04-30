using AutoMapper;
using SGS.Application.DTOs;
using SGS.Application.Helpers;
using SGS.Application.Interfaces;
using SGS.Domain.Entities;
using SGS.Domain.Enums;
using SGS.Infrastructure.Data;
using SGS.Infrastructure.Repositories.Interfaces;
using SGS.Infrastructure.Repositories.UnitOfWork;

namespace SGS.Application.Services.Auth;

public class AuthService(
    IUnitOfWork unitOfWork,
    IMapper mapper,
    ITokenService tokenService,
    IRefreshTokenService refreshTokenService)
    : IAuthService
{
    private readonly IUserRepository _userRepository = unitOfWork.Users;
    
    public async Task<AuthResponseDto> LoginAsync(string email, string password, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByEmailAsync(email, cancellationToken);
        if (user == null || !PasswordHelper.VerifyPassword(password, user.PasswordHash))
            throw new Exception("Invalid credentials.");

        var accessToken = tokenService.GenerateToken(user);
        var refreshToken = await refreshTokenService.CreateRefreshTokenAsync(user.Id, cancellationToken);
        var userDto = mapper.Map<UserDto>(user);

        return new AuthResponseDto
        {
            Token = accessToken,
            RefreshToken = refreshToken.Token,
            User = userDto
        };
    }

    public async Task<UserDto> RegisterUserAsync(RegisterUserCommand command, CancellationToken cancellationToken = default)
    {
        var existingUser = await _userRepository.GetByEmailAsync(command.Email, cancellationToken);
        if (existingUser != null)
            throw new Exception("User with this email already exists.");

        if (!Enum.TryParse(command.Role, true, out UserRole role))
            throw new Exception("Invalid role provided.");

        var hashedPassword = PasswordHelper.HashPassword(command.Password);

        var user = new User
        {
            Id = Guid.NewGuid(),
            FullName = command.FullName,
            Email = command.Email,
            PasswordHash = hashedPassword,
            Role = role
        };

        await _userRepository.CreateAsync(user, cancellationToken);
        return mapper.Map<UserDto>(user);
    }
}