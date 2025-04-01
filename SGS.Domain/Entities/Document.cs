using SGS.Domain.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SGS.Domain.Entities;

public class Document : BaseEntity
{
    [Required]
    public Guid UserId { get; set; }

    [Required]
    public Guid AssignmentId { get; set; }

    [Required]
    public int AttemptNumber { get; set; }

    [Required]
    public string FilePath { get; set; } = null!;

    [Required]
    public DateTime UploadedAt { get; set; }

    [ForeignKey(nameof(UserId))]
    public User User { get; set; } = null!;

    [ForeignKey(nameof(AssignmentId))]
    public Assignment Assignment { get; set; } = null!;
}