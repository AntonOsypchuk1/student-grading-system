using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SGS.Application.DTOs;
using SGS.Application.Interfaces;

namespace SGS.API.Controllers;

[ApiController]
[Route("api/assignments")]
public class AssignmentController(IAssignmentService assignmentService) : ControllerBase
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
        
        if (result.Data is null || !result.IsSuccess)
            return BadRequest(new { error = result.Error });
        
        return CreatedAtAction(nameof(GetAssignmentById), new { id = result.Data.Id }, result.Data);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAssignmentById([FromRoute] Guid id)
    {
        if (id == Guid.Empty)
            return BadRequest(new { error = "Invalid assignment ID." });

        var result = await assignmentService.GetAssignmentByIdAsync(id);
        
        if (!result.IsSuccess)
            return StatusCode(500, new { error = result.Error });
        
        if (result.Data is null)
            return NotFound(new { error = "Assignment not found." });
        
        return Ok(result.Data);
    }

    [HttpGet("group/{groupId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAssignmentsByGroup([FromRoute] Guid groupId)
    {
        if (groupId == Guid.Empty)
            return BadRequest(new { error = "Invalid group ID." });

        var result = await assignmentService.GetAssignmentsByGroupAsync(groupId);
        
        if (!result.IsSuccess)
            return StatusCode(500, new { error = result.Error });
        
        if (result.Data is null)
            return NotFound(new { error = "Group not found." });
        
        if (!result.Data.Any())
            return NoContent();
        
        return Ok(result.Data);
    }
}