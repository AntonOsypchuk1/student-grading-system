using AutoMapper;
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
    [HttpPost("register")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Register([FromBody] RegisterUserCommand command)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var userDto = await authService.RegisterUserAsync(command);
            return CreatedAtAction(nameof(Login), new { id = userDto.Id }, userDto);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("login")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var authResponse = await authService.LoginAsync(dto.Email, dto.Password);
            return Ok(authResponse);
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized(new { error = "Invalid email or password." });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("refresh")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequestDto request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        if (string.IsNullOrEmpty(request.RefreshToken))
            return BadRequest(new { error = "Refresh token is required." });

        var existingRefreshToken = await refreshTokenService.GetRefreshTokenAsync(request.RefreshToken);
        if (existingRefreshToken is not { IsActive: true })
            return Unauthorized(new { error = "Invalid or expired refresh token." });

        try
        {
            await refreshTokenService.RevokeRefreshTokenAsync(request.RefreshToken);

            var user = await userRepository.GetByIdAsync(existingRefreshToken.UserId, CancellationToken.None);
            if (user is null)
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
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}