using System.ComponentModel.DataAnnotations;
using SGS.Domain.Common;

namespace SGS.Domain.Entities;

public class AnalysisResults : BaseEntity
{
    [Required]
    public Guid DocumentId { get; set; }

    [Required]
    public Guid CriteriaId { get; set; }

    [Required]
    public Guid GradeId { get; set; }

    [Required]
    [StringLength(1000)]
    public string GeneratedSummary { get; set; } = null!;

    public bool IsApproved { get; set; }
    public DateTime CreatedAt { get; set; }

    [Required]
    public Document Document { get; set; } = null!;

    [Required]
    public GradingCriteria Criteria { get; set; } = null!;

    [Required]
    public Grade Grade { get; set; } = null!;
}