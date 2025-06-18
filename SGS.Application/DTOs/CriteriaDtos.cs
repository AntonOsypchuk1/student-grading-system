namespace SGS.Application.DTOs;

public class GradingCriteriaDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public double MaxGrade { get; set; }
    public double Weight { get; set; }
    public Guid AssignmentId { get; set; }
}

public class CreateGradingCriteriaBatchCommand
{
    public List<CreateGradingCriteriaCommand> Criteria { get; set; } = [];
}

public class CreateGradingCriteriaCommand
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public double MaxGrade { get; set; }
    public double Weight { get; set; }
    public Guid AssignmentId { get; set; }
}

public class UpdateGradingCriteriaCommand
{
    public Guid Id { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
    public double? MaxGrade { get; set; }
    public double? Weight { get; set; }
}
