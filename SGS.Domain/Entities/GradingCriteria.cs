using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SGS.Domain.Common;

namespace SGS.Domain.Entities;

public class GradingCriteria : BaseEntity
{
    [Required]
    public Guid UserId { get; set; }

    [Required]
    public string CriteriaDescription { get; set; } = null!;

    [Required]
    public DateTime CreatedAt { get; set; }

    [ForeignKey(nameof(UserId))]
    public User User { get; set; } = null!;
}