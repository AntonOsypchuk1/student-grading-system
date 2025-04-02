namespace SGS.Application.DTOs;

public class CreateAssignmentCommand
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int MaxAttempts { get; set; }
    public DateTime DueDate { get; set; }
    public Guid GroupId { get; set; }
    public Guid CourseId { get; set; }
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
}