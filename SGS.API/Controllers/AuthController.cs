using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SGS.API.Controllers.Base;
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
    : BaseApiController
{
    [AllowAnonymous]
    [HttpPost("register")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register([FromBody] RegisterUserCommand command)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await authService.RegisterUserAsync(command);
        return HandleCreatedResult(result, nameof(Login), new { email = command.Email });
    }

    [AllowAnonymous]
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
            return Unauthorized(new ProblemDetails { Detail = result.Error });

        return Ok(result.Data);
    }
    
    // TODO: Implement session management
    // [Authorize]
    // [HttpGet("sessions")]
    // public async Task<IActionResult> GetActiveSessions()
    // {
    //     var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    //     if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var userGuid))
    //         return Unauthorized();
    //
    //     var sessions = await refreshTokenService.GetActiveSessionsForUserAsync(userGuid);
    //     return Ok(sessions);
    // }
    //
    // [Authorize]
    // [HttpDelete("sessions/{sessionId}")]
    // public async Task<IActionResult> TerminateSession(string sessionId)
    // {
    //     var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    //     if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var userGuid))
    //         return Unauthorized();
    //
    //     var result = await refreshTokenService.TerminateSessionAsync(sessionId, userGuid);
    //     return HandleResult(result);
    // }

    
    [Authorize]
    [HttpPost("logout")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Logout([FromBody] RefreshTokenRequestDto request)
    {
        if (string.IsNullOrEmpty(request.RefreshToken))
            return BadRequest(new ProblemDetails { Detail = "Refresh token is required." });

        var tokenResult = await refreshTokenService.GetRefreshTokenAsync(request.RefreshToken);
        if (!tokenResult.IsSuccess)
            return BadRequest(new ProblemDetails { Detail = tokenResult.Error });

        var existingRefreshToken = tokenResult.Data;
        if (existingRefreshToken is not { IsActive: true })
            return Unauthorized(new ProblemDetails { Detail = "Invalid or expired refresh token." });

        var revokeResult = await refreshTokenService.RevokeRefreshTokenAsync(request.RefreshToken);
        if (!revokeResult.IsSuccess)
            return BadRequest(new ProblemDetails { Detail = revokeResult.Error });

        return Ok(new { message = "Logged out successfully" });
    }

    // [Authorize]
    [HttpPost("refresh")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequestDto request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        if (string.IsNullOrEmpty(request.RefreshToken))
            return BadRequest(new ProblemDetails { Detail = "Refresh token is required." });

        var tokenResult = await refreshTokenService.GetRefreshTokenAsync(request.RefreshToken);
        if (!tokenResult.IsSuccess)
            return BadRequest(new ProblemDetails { Detail = tokenResult.Error });

        var existingRefreshToken = tokenResult.Data;
        if (existingRefreshToken is not { IsActive: true })
            return Unauthorized(new ProblemDetails { Detail = "Invalid or expired refresh token." });

        var revokeResult = await refreshTokenService.RevokeRefreshTokenAsync(request.RefreshToken);
        if (!revokeResult.IsSuccess)
            return BadRequest(new ProblemDetails { Detail = revokeResult.Error });
        
        var userResult = await userService.GetUserByIdAsync(existingRefreshToken.UserId);
        if (!userResult.IsSuccess || userResult.Data is null)
            return Unauthorized(new ProblemDetails { Detail = userResult.Error ?? "User not found." });

        var user = userResult.Data;
        var accessToken = tokenService.GenerateToken(mapper.Map<User>(user));

        var newTokenResult = await refreshTokenService.CreateRefreshTokenAsync(user.Id);
        if (!newTokenResult.IsSuccess || newTokenResult.Data is null)
            return BadRequest(new ProblemDetails { Detail = newTokenResult.Error });

        return Ok(new AuthResponseDto
        {
            Token = accessToken,
            RefreshToken = newTokenResult.Data.Token,
            User = user
        });
    }
}