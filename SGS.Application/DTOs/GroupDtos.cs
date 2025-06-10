using System.ComponentModel.DataAnnotations;
using SGS.Domain.Entities;

namespace SGS.Application.DTOs;

public class StudentsGroupBasicDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
}

public class GroupDto : StudentsGroupBasicDto
{
    public int? Year { get; set; }
    public Guid? TeacherId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public ICollection<UserDto> Students { get; set; } = [];
    public ICollection<Course> Courses { get; set; } = [];
    // public ICollection<Teacher> Teachers { get; set; } = [];
}

public class CreateStudentsGroupDto
{
    [Required]
    [StringLength(100, MinimumLength = 3)]
    public string Name { get; set; } = null!;
    
    [StringLength(500)]
    public string? Description { get; set; }
}

public class UpdateStudentsGroupDto
{
    [StringLength(100, MinimumLength = 3)]
    public string? Name { get; set; }
    
    [StringLength(500)]
    public string? Description { get; set; }
}

public class AssignUserToGroupDto
{
    public Guid UserId { get; set; }
    public Guid GroupId { get; set; }
}

public class AssignStudentToGroupDto
{
    public Guid StudentId { get; set; }
    public Guid GroupId { get; set; }
}

