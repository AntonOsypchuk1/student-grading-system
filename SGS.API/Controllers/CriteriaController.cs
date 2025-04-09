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
    public async Task<IActionResult> CreateCriteria([FromBody] CreateGradingCriteriaCommand command)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var criteriaDto = await criteriaService.CreateCriteriaAsync(command);
        return CreatedAtAction(nameof(GetCriteria), new { id = criteriaDto.Id }, criteriaDto);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCriteria(Guid id)
    {
        if (id == Guid.Empty)
            return BadRequest(new { error = "Invalid criteria ID." });

        var criteriaDto = await criteriaService.GetCriteriaByIdAsync(id);
        if (criteriaDto is null)
            return NotFound(new { error = "Criteria not found." });
        return Ok(criteriaDto);
    }

    [HttpGet("all")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> GetAllCriteria()
    {
        var criteriaList = await criteriaService.GetAllCriteriaAsync();
        if (!criteriaList.Any()) return NoContent();
        return Ok(criteriaList);
    }
}