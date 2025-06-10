using System.ComponentModel.DataAnnotations;
using SGS.Domain.Common;

namespace SGS.Domain.Entities;

public class StudentsGroup : BaseEntity
{
    [Key, MaxLength(100)]
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public int? Year { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public ICollection<Student> Students { get; set; } = [];
    public ICollection<Course> Courses { get; set; } = [];
}