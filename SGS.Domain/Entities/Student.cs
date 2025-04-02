using SGS.Domain.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SGS.Domain.Entities;

public class Student : BaseEntity
{
    [Required]
    public Guid UserId { get; set; }
    [Required]
    public Guid GroupId { get; set; }

    [ForeignKey(nameof(UserId))]
    public User User { get; set; } = null!;
    [ForeignKey(nameof(GroupId))]
    public StudentsGroup Group { get; set; } = null!;
}