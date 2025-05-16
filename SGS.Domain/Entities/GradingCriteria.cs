using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SGS.Domain.Common;

namespace SGS.Domain.Entities;

public class GradingCriteria : BaseEntity
{
    [Required]
    public string Title { get; set; }
    
    [Required]
    public string Description { get; set; }
    
    [Required]
    public double MaxGrade { get; set; }
    
    [Required]
    public double Weight { get; set; }
    
    [Required]
    public Guid AssignmentId { get; set; }
    
    [ForeignKey(nameof(AssignmentId))]
    public Assignment Assignment { get; set; } = null!;
}