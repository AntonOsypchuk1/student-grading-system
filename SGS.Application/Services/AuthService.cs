using AutoMapper;
using Microsoft.IdentityModel.Tokens;
using SGS.Application.DTOs;
using SGS.Application.Interfaces;
using SGS.Domain.Entities;
using SGS.Infrastructure.Repositories.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SGS.Application.Services;

public class AuthService(IUserRepository userRepository, IMapper mapper, JwtSettings jwtSettings)
    : IAuthService
{
    public async Task<AuthResponseDto> LoginAsync(string email, string password, CancellationToken cancellationToken = default)
    {
        var user = await userRepository.GetByEmailAsync(email, cancellationToken);
        if (user == null || user.PasswordHash != HashPassword(password))
        {
            throw new Exception("Invalid credentials");
        }

        var token = GenerateJwtToken(user);
        var userDto = mapper.Map<UserDto>(user);

        return new AuthResponseDto { Token = token, User = userDto };
    }

    private string GenerateJwtToken(User user)
    {
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim("fullname", user.FullName),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim("role", user.Role.RoleName)
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Secret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: jwtSettings.Issuer,
            audience: jwtSettings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(jwtSettings.ExpirationInMinutes),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    // TODO: Dummy password hashing (replace with a secure hash)
    private string HashPassword(string password) => $"hashed-{password}";
}
