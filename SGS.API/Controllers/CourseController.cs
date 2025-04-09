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
    public async Task<IActionResult> CreateCourse([FromBody] CreateCourseCommand command)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var courseDto = await courseService.CreateCourseAsync(command);
        return CreatedAtAction(nameof(GetCourseById), new { id = courseDto.Id }, courseDto);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCourseById([FromRoute] Guid id)
    {
        if (id == Guid.Empty)
            return BadRequest(new { error = "Invalid course ID." });

        var courseDto = await courseService.GetCourseByIdAsync(id);
        if (courseDto is null)
            return NotFound(new { error = "Course not found." });
        return Ok(courseDto);
    }

    [Authorize(Policy = "RequireTeacherRole")]
    [HttpGet("teacher/{teacherId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetCoursesByTeacher([FromRoute] Guid teacherId)
    {
        if (teacherId == Guid.Empty)
            return BadRequest(new { error = "Invalid teacher ID." });

        var courses = await courseService.GetCoursesByTeacherAsync(teacherId);
        if (!courses.Any()) return NoContent();
        return Ok(courses);
    }

    [Authorize(Policy = "RequireStudentRole")]
    [HttpGet("student/{studentId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetCoursesByStudent([FromRoute] Guid studentId)
    {
        if (studentId == Guid.Empty)
            return BadRequest(new { error = "Invalid student ID." });

        var courses = await courseService.GetCoursesByStudentAsync(studentId);
        if (!courses.Any()) return NoContent();
        return Ok(courses);
    }
}