using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SGS.Application.DTOs;
using SGS.Application.Interfaces;

namespace SGS.API.Controllers;

[ApiController]
[Route("api/criteria")]
public class CriteriaController(IGradingCriteriaService criteriaService) : ControllerBase
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
        
        if (!result.IsSuccess || result.Data is null)
            return BadRequest(new { error = result.Error });
        
        return CreatedAtAction(nameof(GetCriteria), new { id = result.Data.Id }, result.Data);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetCriteria(Guid id)
    {
        if (id == Guid.Empty)
            return BadRequest(new { error = "Invalid criteria ID." });

        var result = await criteriaService.GetCriteriaByIdAsync(id);
        
        if (!result.IsSuccess)
            return StatusCode(500, new { error = result.Error });
            
        if (result.Data is null)
            return NotFound(new { error = "Criteria not found." });
            
        return Ok(result.Data);
    }

    [HttpGet("all")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> GetAllCriteria()
    {
        var result = await criteriaService.GetAllCriteriaAsync();
        
        if (!result.IsSuccess)
            return StatusCode(500, new { error = result.Error });
            
        if (result.Data is null || !result.Data.Any()) 
            return NoContent();
            
        return Ok(result.Data);
    }
}