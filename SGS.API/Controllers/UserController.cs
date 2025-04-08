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
    public async Task<IActionResult> GetCurrentUser()
    {
        var email = User.FindFirst(ClaimTypes.Email)?.Value;
        if (string.IsNullOrEmpty(email))
            return Unauthorized(new { error = "Invalid token." });
        
        var userDto = await userService.GetUserByEmailAsync(email);
        if (userDto == null) return NotFound();
        return Ok(userDto);
    }

    [HttpGet("teachers")]
    public async Task<IActionResult> GetTeachers()
    {
        var teachers = await userService.GetTeachersAsync();
        return Ok(teachers);
    }
        
    [HttpGet("students")]
    public async Task<IActionResult> GetStudents()
    {
        var students = await userService.GetStudentsAsync();
        return Ok(students);
    }
        
    [HttpGet("groups/{groupId:guid}")]
    public async Task<IActionResult> GetStudentsByGroup(Guid groupId)
    {
        var groupStudents = await userService.GetStudentsByGroupAsync(groupId);
        return Ok(groupStudents);
    }
}