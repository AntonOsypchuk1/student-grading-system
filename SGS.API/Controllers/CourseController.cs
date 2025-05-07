using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SGS.Application.DTOs;
using SGS.Application.Interfaces;

namespace SGS.API.Controllers;

[ApiController]
[Route("api/courses")]
public class CourseController(ICourseService courseService) : ControllerBase
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
        
        if (result.Data is null || !result.IsSuccess)
            return BadRequest(new { error = result.Error });
        
        return CreatedAtAction(nameof(GetCourseById), new { id = result.Data.Id }, result.Data);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetCourseById([FromRoute] Guid id)
    {
        if (id == Guid.Empty)
            return BadRequest(new { error = "Invalid course ID." });

        var result = await courseService.GetCourseByIdAsync(id);
        
        if (!result.IsSuccess)
            return StatusCode(500, new { error = result.Error });
        
        if (result.Data is null)
            return NotFound(new { error = "Course not found." });
        
        return Ok(result.Data);
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
        if (teacherId == Guid.Empty)
            return BadRequest(new { error = "Invalid teacher ID." });

        var result = await courseService.GetCoursesByTeacherAsync(teacherId);
        
        if (!result.IsSuccess)
            return StatusCode(500, new { error = result.Error });
        
        if (result.Data is null || !result.Data.Any()) 
            return NoContent();
        
        return Ok(result.Data);
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
        if (studentId == Guid.Empty)
            return BadRequest(new { error = "Invalid student ID." });

        var result = await courseService.GetCoursesByStudentAsync(studentId);
        
        if (!result.IsSuccess)
            return StatusCode(500, new { error = result.Error });
        
        if (result.Data is null || !result.Data.Any())
            return NoContent();
        
        return Ok(result.Data);
    }
}