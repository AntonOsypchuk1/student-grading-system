using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SGS.API.Controllers.Base;
using SGS.API.Extensions;
using SGS.Application.DTOs;
using SGS.Application.Interfaces;

namespace SGS.API.Controllers;

[ApiController]
[Route("api/courses")]
public class CourseController(ICourseService courseService) : BaseApiController
{
    [Authorize(Policy = "RequireTeacherRole")]
    [HttpPost("create")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateCourse([FromBody] CreateCourseCommand command)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);
        
        var result = await courseService.CreateCourseAsync(command);
        return HandleCreatedResult(result, nameof(GetCourseById), new { id = result.Data?.Id });
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetCourseById([FromRoute] Guid id)
    {
        if (!this.IsValidGuid(id, out var errorResult))
            return errorResult;

        var result = await courseService.GetCourseByIdAsync(id);
        return HandleResult(result);
    }

    [Authorize(Policy = "RequireTeacherRole")]
    [HttpGet("teacher/{teacherId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetCoursesByTeacher([FromRoute] Guid teacherId)
    {
        if (!this.IsValidGuid(teacherId, out var errorResult))
            return errorResult;

        var result = await courseService.GetCoursesByTeacherAsync(teacherId);
        return HandleCollectionResult(result);
    }

    [Authorize(Policy = "RequireStudentRole")]
    [HttpGet("student/{studentId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetCoursesByStudent([FromRoute] Guid studentId)
    {
        if (!this.IsValidGuid(studentId, out var errorResult))
            return errorResult;

        var result = await courseService.GetCoursesByStudentAsync(studentId);
        return HandleCollectionResult(result);
    }
}