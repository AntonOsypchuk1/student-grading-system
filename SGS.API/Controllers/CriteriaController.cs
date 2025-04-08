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
    public async Task<IActionResult> CreateCriteria([FromBody] CreateGradingCriteriaCommand command)
    {
        var criteriaDto = await criteriaService.CreateCriteriaAsync(command);
        return Ok(criteriaDto);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetCriteria(Guid id)
    {
        var criteriaDto = await criteriaService.GetCriteriaByIdAsync(id);
        if (criteriaDto == null)
            return NotFound();
        return Ok(criteriaDto);
    }

    [HttpGet("all")]
    public async Task<IActionResult> GetAllCriteria()
    {
        var criteriaList = await criteriaService.GetAllCriteriaAsync();
        return Ok(criteriaList);
    }
}