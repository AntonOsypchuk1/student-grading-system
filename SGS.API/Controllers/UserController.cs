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
    public async Task<IActionResult> GetCurrentUser()
    {
        var email = User.FindFirst(ClaimTypes.Email)?.Value;
        if (string.IsNullOrEmpty(email))
            return Unauthorized(new { error = "Invalid token." });
        
        var userDto = await userService.GetUserByEmailAsync(email);
        if (userDto == null) return NotFound(new { error = "User not found." });
        return Ok(userDto);
    }

    [HttpGet("teachers")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> GetTeachers()
    {
        var teachers = await userService.GetTeachersAsync();
        if (!teachers.Any()) return NoContent();
        return Ok(teachers);
    }

    [HttpGet("students")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> GetStudents()
    {
        var students = await userService.GetStudentsAsync();
        if (!students.Any()) return NoContent();
        return Ok(students);
    }

    [HttpGet("groups/{groupId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetStudentsByGroup(Guid groupId)
    {
        if (groupId == Guid.Empty)
            return BadRequest(new { error = "Invalid group ID." });
            
        var groupStudents = await userService.GetStudentsByGroupAsync(groupId);
        if (groupStudents is null) return NotFound(new { error = "Group not found." });
        if (!groupStudents.Any()) return NoContent();
        return Ok(groupStudents);
    }
}