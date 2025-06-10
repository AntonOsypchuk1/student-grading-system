using System.ComponentModel.DataAnnotations;

namespace SGS.Application.DTOs;

public class CreateAssignmentCommand
{
    public Guid GroupId { get; set; }
    public Guid CourseId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int MaxAttempts { get; set; }
    public DateTime DueDate { get; set; }
    public ICollection<CreateGradingCriteriaCommand>? GradingCriteria { get; set; }
}

public class UpdateAssignmentDto
{
    [StringLength(100, MinimumLength = 3)]
    public string? Title { get; set; }
    
    [StringLength(1000, MinimumLength = 10)]
    public string? Description { get; set; }
    
    [Range(1, 10)]
    public int? MaxAttempts { get; set; }
    
    public DateTime? DueDate { get; set; }
}


public class AssignmentDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int MaxAttempts { get; set; }
    public DateTime DueDate { get; set; }
    public Guid GroupId { get; set; }
    public Guid CourseId { get; set; }
    
    public CourseBasicDto Course { get; set; } = null!;
    public StudentsGroupBasicDto Group { get; set; } = null!;
    public ICollection<GradingCriteriaDto> GradingCriteria { get; set; } = [];

}