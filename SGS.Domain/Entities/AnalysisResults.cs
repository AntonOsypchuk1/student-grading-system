using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SGS.Domain.Common;

namespace SGS.Domain.Entities;

public class AnalysisResults : BaseEntity
{
    [Required]
    public Guid DocumentId { get; set; }

    [Required]
    public Guid GradeId { get; set; }

    [Required, StringLength(2000)]
    public string GeneratedSummary { get; set; } = null!;
    
    [Required]
    public List<string> Strengths { get; set; } = [];
    
    [Required]
    public List<string> AreasForImprovement { get; set; } = [];

    [Required] 
    public bool IsApproved { get; set; }
    
    public DateTime CreatedAt { get; set; }

    [ForeignKey(nameof(DocumentId))]
    public Document Document { get; set; } = null!;

    [ForeignKey(nameof(GradeId))]
    public Grade Grade { get; set; } = null!;
    
    public ICollection<GradingCriteria> Criterias { get; set; } = [];
}