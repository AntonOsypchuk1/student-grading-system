using SGS.Domain.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SGS.Domain.Entities;

public class Teacher : BaseEntity
{
    [Required]
    public Guid UserId { get; set; }

    [ForeignKey(nameof(UserId))]
    public User User { get; set; } = null!;

    public ICollection<Course> Courses { get; set; } = new List<Course>();
    public ICollection<StudentsGroup> StudentsGroups { get; set; } = new List<StudentsGroup>();
}