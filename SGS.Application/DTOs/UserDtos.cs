using System.ComponentModel.DataAnnotations;

namespace SGS.Application.DTOs;

public class RegisterUserCommand
{
    public string Login { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
}

public class UpdateUserDto
{
    public string? FullName { get; set; }
    
    [EmailAddress]
    public string? Email { get; set; }
    
    // For students, they can be moved to a different group
    public Guid? GroupId { get; set; }
}


public class UserDto
{
    public Guid Id { get; set; }
    public Guid? GroupId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    
    public StudentsGroupBasicDto? Group { get; set; }
    
    // For teachers, include courses they teach
    public ICollection<CourseBasicDto>? Courses { get; set; }
}

public class TeacherDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string FullName { get; set; } = null!;
    public string Email { get; set; } = null!;

    public ICollection<CourseDto> Courses { get; set; } = [];
}
