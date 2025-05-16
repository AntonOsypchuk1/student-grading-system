using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SGS.API.Controllers.Base;
using SGS.API.Extensions;
using SGS.Application.DTOs;
using SGS.Application.Interfaces;

namespace SGS.API.Controllers;

[ApiController]
[Route("api/criteria")]
public class CriteriaController(IGradingCriteriaService criteriaService) : BaseApiController
{
    [Authorize(Policy = "RequireTeacherRole")]
    [HttpPost("create")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateCriteria([FromBody] CreateGradingCriteriaCommand command)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await criteriaService.CreateCriteriaAsync(command);
        return HandleCreatedResult(result, nameof(GetCriteria), new { id = result.Data?.Id });}

    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetCriteria(Guid id)
    {
        if (!this.IsValidGuid(id, out var errorResult))
            return errorResult;
        
        var result = await criteriaService.GetCriteriaByIdAsync(id);
        return HandleResult(result);
    }

    [HttpGet("all")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> GetAllCriteria()
    {
        var result = await criteriaService.GetAllCriteriaAsync();
        return HandleCollectionResult(result);
    }
    
    [HttpGet("by-assignment/{assignmentId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetCriteriaByAssignment(Guid assignmentId)
    {
        if (!this.IsValidGuid(assignmentId, out var errorResult))
            return errorResult; 
        
        var result = await criteriaService.GetCriteriaByAssignmentAsync(assignmentId);
        return HandleCollectionResult(result);
    }
}