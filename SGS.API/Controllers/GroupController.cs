using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SGS.API.Controllers.Base;
using SGS.API.Extensions;
using SGS.Application.DTOs;
using SGS.Application.Interfaces;

namespace SGS.API.Controllers;

[Authorize]
[ApiController]
[Route("api/groups")]
public class GroupController(
    IStudentsGroupService groupService,
    IUserService userService
    ) : BaseApiController
{
    [HttpPost("{groupId:guid}/student/{studentId:guid}")]
    // [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> AssignStudentToGroup([FromRoute] Guid groupId, [FromRoute] Guid studentId,
        CancellationToken cancellationToken = default)
    {
        if (!this.IsValidGuid(groupId, out var groupErrorResult))
            return groupErrorResult;

        if (!this.IsValidGuid(studentId, out var studentErrorResult))
            return studentErrorResult;

        var result = await groupService.AssignUserToGroupAsync(studentId, groupId, cancellationToken);

        if (!result.IsSuccess)
            return BadRequest(new ProblemDetails { Detail = result.Error });

        return Ok(result.Data);
    }

    // [Authorize(Policy = "RequireTeacherRole")]
    // [HttpDelete("{groupId:guid}/students/{studentId:guid}")]
    // [ProducesResponseType(StatusCodes.Status200OK)]
    // [ProducesResponseType(StatusCodes.Status400BadRequest)]
    // [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    // [ProducesResponseType(StatusCodes.Status403Forbidden)]
    // [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    // public async Task<IActionResult> RemoveStudentFromGroup([FromRoute] Guid groupId, [FromRoute] Guid studentId)
    // {
    //     if (!this.IsValidGuid(groupId, out var groupErrorResult))
    //         return groupErrorResult;
    //         
    //     if (!this.IsValidGuid(studentId, out var studentErrorResult))
    //         return studentErrorResult;
    //     
    //     var result = await groupService.RemoveUserFromGroupAsync(studentId, groupId);
    //     return HandleResult(result);
    // }

    // [Authorize(Roles = "Admin")]
    [HttpGet("all")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAllGroups(CancellationToken cancellationToken = default)
    {
        var result = await groupService.GetAllGroupsAsync(cancellationToken);
        return HandleCollectionResult(result);
    }
    
    // [HttpGet("student/{userId:guid}")]
    // [ProducesResponseType(StatusCodes.Status200OK)]
    // [ProducesResponseType(StatusCodes.Status404NotFound)]
    // [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    // public async Task<IActionResult> GetGroupByUserId(Guid userId, CancellationToken cancellationToken = default)
    // {
    //     var result = await groupService.GetGroupByUserIdAsync(userId, cancellationToken);
    //     return HandleResult(result);
    // }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetGroupById(Guid id, CancellationToken cancellationToken = default)
    {
        var result = await groupService.GetGroupByIdAsync(id, cancellationToken);
        return HandleResult(result);
    }

    [HttpGet("student/me")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetGroupByStudentId(CancellationToken cancellationToken = default)
    {
        var email = User.FindFirst(ClaimTypes.Email)?.Value
                    ?? User.FindFirst(JwtRegisteredClaimNames.Email)?.Value;
        if (string.IsNullOrEmpty(email))
            return Unauthorized(new { error = "Invalid token." });
        
        var userResult = await userService.GetUserByEmailAsync(email, cancellationToken);
        if (!userResult.IsSuccess || userResult.Data is null)
            return NotFound(new ProblemDetails { Detail = "User not found." });
        
        var result = await groupService.GetGroupByStudentIdAsync(userResult.Data.Id, cancellationToken);
        return HandleResult(result);
    }

    [HttpGet("name/{name}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetByGroupName(string name, CancellationToken cancellationToken = default)
    {
        var result = await groupService.GetByGroupByNameAsync(name, cancellationToken);
        return HandleResult(result);
    }
    
    [HttpGet("course/{courseId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetByCourseId([FromRoute] Guid courseId)
    {
        if (!this.IsValidGuid(courseId, out var errorResult))
            return errorResult;
    
        var result = await groupService.GetGroupByCourseIdAsync(courseId);
        return HandleCollectionResult(result);
    }

    [HttpPost]
    // [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CreateGroup([FromBody] CreateStudentsGroupDto groupDto, CancellationToken cancellationToken = default)
    {
        var result = await groupService.CreateGroupAsync(groupDto, cancellationToken);
        return HandleCreatedResult(result, nameof(GetGroupById), new { id = result.IsSuccess ? result.Data?.Id : Guid.Empty });
    }

    [HttpPut("{id:guid}")]
    // [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateGroup(Guid id, [FromBody] GroupDto groupDto, CancellationToken cancellationToken = default)
    {
        if (id != groupDto.Id)
            return BadRequest(new ProblemDetails { Detail = "The ID in the URL must match the ID in the request body." });

        var result = await groupService.UpdateGroupAsync(id, groupDto, cancellationToken);
        return HandleResult(result);
    }

    [HttpDelete("{id:guid}")]
    // [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteGroup(Guid id, CancellationToken cancellationToken = default)
    {
        var result = await groupService.DeleteGroupAsync(id, cancellationToken);
        
        if (!result.IsSuccess)
            return HandleResult(result);
            
        return NoContent();
    }
}