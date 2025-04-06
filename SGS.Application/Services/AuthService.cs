using AutoMapper;
using SGS.Application.DTOs;
using SGS.Application.Interfaces;
using SGS.Infrastructure.Repositories.Interfaces;

namespace SGS.Application.Services;

public class AuthService(
    IUserRepository userRepository,
    IMapper mapper,
    ITokenService tokenService,
    IRefreshTokenService refreshTokenService)
    : IAuthService
{
    public async Task<AuthResponseDto> LoginAsync(string email, string password, CancellationToken cancellationToken = default)
    {
        var user = await userRepository.GetByEmailAsync(email, cancellationToken);
        if (user == null || user.PasswordHash != HashPassword(password))
        {
            throw new Exception("Invalid credentials");
        }

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

    // TODO: Dummy implementation for password hashing – replace with a secure one
    private static string HashPassword(string password) => $"hashed-{password}";
}