using SGS.Domain.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SGS.Domain.Entities;

public class Assignment : BaseEntity
{
    [Required]
    public Guid GroupId { get; set; }

    [Required]
    public Guid CourseId { get; set; }

    [Required]
    public string Title { get; set; } = null!;

    [Required]
    public string Description { get; set; } = null!;

    [Required]
    public int MaxAttempts { get; set; }

    [Required]
    public DateTime DueDate { get; set; }

    [ForeignKey(nameof(GroupId))]
    public StudentsGroup Group { get; set; } = null!;

    [ForeignKey(nameof(CourseId))]
    public Course Course { get; set; } = null!;

    public ICollection<Document> Documents { get; set; } = new List<Document>();
}