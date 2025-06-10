using System.ComponentModel.DataAnnotations;

namespace SGS.Application.DTOs;

public class CourseBasicDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = null!;
    public string Description { get; set; } = null!;
}

public class CourseDto : CourseBasicDto
{
    public ICollection<TeacherDto> Teachers { get; set; } = [];
    public ICollection<StudentsGroupBasicDto> StudentsGroups { get; set; } = [];
}

public class CreateCourseCommand
{
    [StringLength(100, MinimumLength = 3)]
    public string Title { get; set; } = string.Empty;
    
    [StringLength(500, MinimumLength = 10)]
    public string Description { get; set; } = string.Empty;
}

public class UpdateCourseDto
{
    [StringLength(100, MinimumLength = 3)]
    public string? Title { get; set; }
    
    [StringLength(500, MinimumLength = 10)]
    public string? Description { get; set; }
}


public class AssignTeacherToCourseDto
{
    public Guid TeacherId { get; set; }
    public Guid CourseId { get; set; }
}

public class AssignGroupToCourseDto
{
    public Guid GroupId { get; set; }
    public Guid CourseId { get; set; }
}

