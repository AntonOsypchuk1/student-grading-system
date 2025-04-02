namespace SGS.Application.DTOs;

public class CourseDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

public class CreateCourseCommand
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}