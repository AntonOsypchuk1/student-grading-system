namespace SGS.Application.DTOs;

public class AnalysisResultDto
{
    public Guid Id { get; set; }
    public Guid DocumentId { get; set; }
    public Guid CriteriaId { get; set; }
    public Guid GradeId { get; set; }
    public string GeneratedSummary { get; set; } = string.Empty;
    public bool IsApproved { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class EditGradeCommand
{
    public Guid GradeId { get; set; }
    public int EditedGrade { get; set; }
    public bool IsFinalized { get; set; }
}