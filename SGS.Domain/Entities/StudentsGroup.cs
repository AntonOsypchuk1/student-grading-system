using SGS.Domain.Common;
using System.ComponentModel.DataAnnotations;

namespace SGS.Domain.Entities;

public class StudentsGroup : BaseEntity
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = null!;

    public ICollection<Student> Students { get; set; } = new List<Student>();
    public ICollection<Course> Courses { get; set; } = new List<Course>();
    public ICollection<Teacher> Teachers { get; set; } = new List<Teacher>();
}