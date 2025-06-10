using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SGS.API.Controllers.Base;
using SGS.API.Extensions;
using SGS.Application.Interfaces;

namespace SGS.API.Controllers;

[Authorize]
[ApiController]
[Route("api/users")]
public class UserController(IUserService userService) : BaseApiController
{
    [HttpGet("me")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetCurrentUser()
    {
        var email = User.FindFirst(ClaimTypes.Email)?.Value
            ?? User.FindFirst(JwtRegisteredClaimNames.Email)?.Value;
        if (string.IsNullOrEmpty(email))
            return Unauthorized(new { error = "Invalid token." });
        
        var result = await userService.GetUserByEmailAsync(email);
        return HandleResult(result);
    }

    [Authorize(Policy = "RequireTeacherRole")]
    [HttpGet("teachers")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetTeachers()
    {
        var result = await userService.GetTeachersAsync();
        return HandleCollectionResult(result);
    }

    // [Authorize(Policy = "RequireTeacherRole")]
    [HttpGet("students")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> GetStudents()
    {
        var result = await userService.GetStudentsAsync();
        return HandleCollectionResult(result);
    }
}