namespace SGS.Application.DTOs;

public class GradingCriteriaDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string CriteriaDescription { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class CreateGradingCriteriaCommand
{
    public Guid UserId { get; set; }  // Teacher's ID
    public string CriteriaDescription { get; set; } = string.Empty;
}