using Microsoft.AspNetCore.Mvc;
using SGS.Application.DTOs;
using SGS.Application.Interfaces;

namespace SGS.API.Controllers;

[ApiController]
[Route("api/assignments")]
public class AssignmentController(IAssignmentService assignmentService) : ControllerBase
{
    [HttpPost("create")]
    public async Task<IActionResult> CreateAssignment([FromBody] CreateAssignmentCommand command)
    {
        var assignmentDto = await assignmentService.CreateAssignmentAsync(command);
        return Ok(assignmentDto);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetAssignmentById([FromRoute] Guid id)
    {
        var assignmentDto = await assignmentService.GetAssignmentByIdAsync(id);
        if (assignmentDto == null)
            return NotFound();
        return Ok(assignmentDto);
    }

    [HttpGet("group/{groupId:guid}")]
    public async Task<IActionResult> GetAssignmentsByGroup([FromRoute] Guid groupId)
    {
        var assignments = await assignmentService.GetAssignmentsByGroupAsync(groupId);
        return Ok(assignments);
    }
}