using System.Security.Claims;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SGS.API.DTOs;
using SGS.Application.DTOs;
using SGS.Application.Interfaces;
using SGS.Infrastructure.Repositories.Interfaces;

namespace SGS.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(
    IAuthService authService, 
    ITokenService tokenService,
    IRefreshTokenService refreshTokenService,
    IUserRepository userRepository,
    IMapper mapper)
    : ControllerBase
{
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        try
        {
            var authResponse = await authService.LoginAsync(dto.Email, dto.Password);
            return Ok(authResponse);
        }
        catch (Exception ex)
        {
            return Unauthorized(new { error = ex.Message });
        }
    }
    
    [Authorize]
    [HttpGet("me")]
    public IActionResult GetCurrentUser()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        var fullName = User.FindFirst("fullname")?.Value;
        var email = User.FindFirst(ClaimTypes.Email)?.Value;
        var role = User.FindFirst("role")?.Value;

        if (userIdClaim == null)
        {
            return Unauthorized(new { error = "Invalid token." });
        }

        var userDto = new UserDto
        {
            Id = Guid.Parse(userIdClaim.Value),
            FullName = fullName ?? string.Empty,
            Email = email ?? string.Empty,
            Role = role ?? string.Empty
        };

        return Ok(userDto);
    }
    
    [HttpPost("refresh")]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequestDto request)
    {
        var existingRefreshToken = await refreshTokenService.GetRefreshTokenAsync(request.RefreshToken);
        if (existingRefreshToken is not { IsActive: true })
        {
            return Unauthorized(new { error = "Invalid or expired refresh token." });
        }

        await refreshTokenService.RevokeRefreshTokenAsync(request.RefreshToken);

        var user = await userRepository.GetByIdAsync(existingRefreshToken.UserId, CancellationToken.None);
        if (user == null)
            return Unauthorized(new { error = "User not found." });

        var accessToken = tokenService.GenerateToken(user);
        var newRefreshToken = await refreshTokenService.CreateRefreshTokenAsync(user.Id);

        return Ok(new AuthResponseDto
        {
            Token = accessToken,
            RefreshToken = newRefreshToken.Token,
            User = mapper.Map<UserDto>(user)
        });
    }
}
