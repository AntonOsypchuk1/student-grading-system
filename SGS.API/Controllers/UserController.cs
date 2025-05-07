using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SGS.Application.Interfaces;

namespace SGS.API.Controllers;

[ApiController]
[Route("api/users")]
public class UserController(IUserService userService) : ControllerBase
{
    [Authorize]
    [HttpGet("me")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetCurrentUser()
    {
        var email = User.FindFirst(ClaimTypes.Email)?.Value;
        if (string.IsNullOrEmpty(email))
            return Unauthorized(new { error = "Invalid token." });
        
        var result = await userService.GetUserByEmailAsync(email);
        
        if (!result.IsSuccess)
            return StatusCode(500, new { error = result.Error });
        
        if (result.Data == null)
            return NotFound(new { error = "User not found." });
        
        return Ok(result.Data);
    }

    [HttpGet("teachers")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetTeachers()
    {
        var result = await userService.GetTeachersAsync();
        
        if (!result.IsSuccess)
            return StatusCode(500, new { error = result.Error });
        
        if (result.Data is null || !result.Data.Any())
            return NoContent();
        
        return Ok(result.Data);
    }

    [HttpGet("students")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> GetStudents()
    {
        var result = await userService.GetStudentsAsync();
        
        if (!result.IsSuccess)
            return StatusCode(500, new { error = result.Error });
        
        if (result.Data is null || !result.Data.Any())
            return NoContent();
        
        return Ok(result.Data);
    }

    [HttpGet("groups/{groupId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetStudentsByGroup(Guid groupId)
    {
        if (groupId == Guid.Empty)
            return BadRequest(new { error = "Invalid group ID." });

        var result = await userService.GetStudentsByGroupAsync(groupId);
        
        if (!result.IsSuccess)
            return StatusCode(500, new { error = result.Error });
            
        if (result.Data == null) 
            return NotFound(new { error = "Group not found." });
            
        if (!result.Data.Any()) 
            return NoContent();
            
        return Ok(result.Data);
    }
}