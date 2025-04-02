using Microsoft.AspNetCore.Mvc;
using SGS.Application.DTOs;
using SGS.Application.Interfaces;

namespace SGS.API.Controllers;

[ApiController]
[Route("api/courses")]
public class CourseController(ICourseService courseService) : ControllerBase
{
    [HttpPost("create")]
    public async Task<IActionResult> CreateCourse([FromBody] CreateCourseCommand command)
    {
        var courseDto = await courseService.CreateCourseAsync(command);
        return Ok(courseDto);
    }
    
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetCourseById([FromRoute] Guid id)
    {
        var courseDto = await courseService.GetCourseByIdAsync(id);
        if (courseDto == null)
            return NotFound();
        return Ok(courseDto);
    }
    
    [HttpGet("teacher/{teacherId:guid}")]
    public async Task<IActionResult> GetCoursesByTeacher([FromRoute] Guid teacherId)
    {
        var courses = await courseService.GetCoursesByTeacherAsync(teacherId);
        return Ok(courses);
    }
    
    [HttpGet("student/{studentId:guid}")]
    public async Task<IActionResult> GetCoursesByStudent([FromRoute] Guid studentId)
    {
        var courses = await courseService.GetCoursesByStudentAsync(studentId);
        return Ok(courses);
    }
}