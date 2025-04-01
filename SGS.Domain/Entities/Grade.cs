using SGS.Domain.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SGS.Domain.Entities;

public class Grade : BaseEntity
{
    [Required]
    public Guid DocumentId { get; set; }

    [Required]
    public int GeneratedGrade { get; set; }

    public int? EditedGrade { get; set; }

    [Required]
    public bool IsFinalized { get; set; }

    [ForeignKey(nameof(DocumentId))]
    public Document Document { get; set; } = null!;
}