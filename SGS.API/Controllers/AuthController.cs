using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using SGS.API.DTOs;
using SGS.Application.DTOs;
using SGS.Application.Interfaces;
using SGS.Domain.Entities;

namespace SGS.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(
    IAuthService authService,
    ITokenService tokenService,
    IRefreshTokenService refreshTokenService,
    IUserService userService,
    IMapper mapper)
    : ControllerBase
{
    // TODO: Implement logout functionality
    
    [HttpPost("register")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register([FromBody] RegisterUserCommand command)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await authService.RegisterUserAsync(command);
        
        if (result.IsSuccess)
            return CreatedAtAction(nameof(Login), new { id = result.Data?.Id }, result.Data);

        return BadRequest(new { error = result.Error });
    }

    [HttpPost("login")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await authService.LoginAsync(dto.Email, dto.Password);
        
        if (!result.IsSuccess)
            return Unauthorized(new { error = result.Error });

        return Ok(result.Data);
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

        var tokenResult = await refreshTokenService.GetRefreshTokenAsync(request.RefreshToken);
        if (!tokenResult.IsSuccess)
            return BadRequest(new { error = tokenResult.Error });
        
        var existingRefreshToken = tokenResult.Data;
        if (existingRefreshToken is not { IsActive: true })
            return Unauthorized(new { error = "Invalid or expired refresh token." });

        var revokeResult = await refreshTokenService.RevokeRefreshTokenAsync(request.RefreshToken);
        if (!revokeResult.IsSuccess)
            return BadRequest(new { error = revokeResult.Error });
        
        var userResult = await userService.GetUserByIdAsync(existingRefreshToken.UserId);
        if (!userResult.IsSuccess || userResult.Data == null)
            return Unauthorized(new { error = userResult.Error ?? "User not found." });

        var user = userResult.Data;
        var accessToken = tokenService.GenerateToken(mapper.Map<User>(user));
        
        var newTokenResult = await refreshTokenService.CreateRefreshTokenAsync(user.Id);
        if (!newTokenResult.IsSuccess || newTokenResult.Data == null)
            return BadRequest(new { error = newTokenResult.Error });
        
        return Ok(new AuthResponseDto
        {
            Token = accessToken,
            RefreshToken = newTokenResult.Data.Token,
            User = user
        });
    }
}