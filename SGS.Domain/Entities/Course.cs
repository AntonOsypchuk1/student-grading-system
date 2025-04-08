using System.ComponentModel.DataAnnotations;
using SGS.Domain.Common;

namespace SGS.Domain.Entities;

public class Course : BaseEntity
{
    [Required]
    public string Title { get; set; } = null!;
    [Required]
    public string Description { get; set; } = null!;
    
    public ICollection<Teacher> Teachers { get; set; } = [];
    public ICollection<StudentsGroup> StudentsGroups { get; set; } = [];
}