namespace SGS.Application.DTOs;

public class AnalysisResultDto
{
    public Guid Id { get; set; }
    public Guid DocumentId { get; set; }
    public Guid CriteriaId { get; set; }
    public Guid GradeId { get; set; }
    public bool IsApproved { get; set; }
    public DateTime CreatedAt { get; set; }
    public string GeneratedSummary { get; set; } = string.Empty;
    public List<string> Strengths { get; set; } = [];
    public List<string> AreasForImprovement { get; set; } = [];
    public GradeDto? Grade { get; set; } = new();
}

public class GradingResultDto
{
    public Guid Id { get; set; }
    public Guid AssignmentId { get; set; }
    public Guid StudentId { get; set; }
    public Guid DocumentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public int? GeneratedGrade { get; set; }
    public int? EditedGrade { get; set; }
    public bool? IsFinalized { get; set; }
    public DateTime? SubmissionDate { get; set; }
}

public class CourseGradingDto
{
    public Guid CourseId { get; set; }
    public string CourseName { get; set; } = string.Empty;
    public Guid GroupId { get; set; }
    public string GroupName { get; set; } = string.Empty;
    public List<AssignmentResultsDto> AssignmentResults { get; set; } = [];
}

public class AssignmentResultsDto
{
    public Guid Id { get; set; }
    public List<GradingResultDto> Results { get; set; } = [];
}

public class GradeDto
{
    public Guid Id { get; set; }
    public Guid DocumentId { get; set; }
    public Guid AssignmentId { get; set; }
    public int GeneratedGrade { get; set; }
    public int? EditedGrade { get; set; }
    public bool IsFinalized { get; set; }
}

public class EditGradeCommand
{
    public Guid GradeId { get; set; }
    public int EditedGrade { get; set; }
    public bool IsFinalized { get; set; }
}