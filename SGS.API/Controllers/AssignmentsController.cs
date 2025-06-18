using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SGS.API.Controllers.Base;
using SGS.API.Extensions;
using SGS.Application.DTOs;
using SGS.Application.Interfaces;

namespace SGS.API.Controllers;

[Authorize]
[ApiController]
[Route("api/assignments")]
public class AssignmentController(IAssignmentService assignmentService) : BaseApiController
{
    [Authorize(Policy = "RequireTeacherRole")]
    [HttpPost("create")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateAssignment([FromBody] CreateAssignmentCommand command)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await assignmentService.CreateAssignmentAsync(command);
        return HandleCreatedResult(result, nameof(GetAssignmentById), new { id = result.Data?.Id });
    }
    
    [Authorize(Policy = "RequireTeacherRole")]
    [HttpPut("update/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateAssignment([FromRoute] Guid id, [FromBody] UpdateAssignmentCommand command)
    {
        if (!this.IsValidGuid(id, out var errorResult))
            return errorResult;

        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        command.Id = id;
        var result = await assignmentService.UpdateAssignmentAsync(command);
        return HandleCreatedResult(result, nameof(GetAssignmentById), new { id = result.Data?.Id });
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAssignmentById([FromRoute] Guid id)
    {
        if (!this.IsValidGuid(id, out var errorResult))
            return errorResult;
        
        var result = await assignmentService.GetAssignmentByIdAsync(id);
        return HandleResult(result);
    }

    [HttpGet("course")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAssignmentsByGroupAndCourse([FromQuery] Guid groupId, [FromQuery] Guid courseId)
    {
        if (!this.IsValidGuid(courseId, out var errorResult))
            return errorResult;
        
        if (groupId != Guid.Empty)
        {
            if (!this.IsValidGuid(groupId, out errorResult))
                return errorResult;

            var result = await assignmentService.GetAssignmentsByGroupAndCourseAsync(groupId, courseId);
            return HandleCollectionResult(result);
        }
        else
        {
            var result = await assignmentService.GetAssignmentsByCourseAsync(courseId);
            return HandleCollectionResult(result);
        }
    }
    
    [HttpGet("group/{groupId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAssignmentsByGroup([FromRoute] Guid groupId)
    {
        if (!this.IsValidGuid(groupId, out var errorResult))
            return errorResult;

        var result = await assignmentService.GetAssignmentsByGroupAsync(groupId);
        return HandleCollectionResult(result);
    }
}