using System.ComponentModel.DataAnnotations;

namespace SGS.Domain.Common;

public class BaseEntity
{
    [Key]
    public Guid Id { get; set; }
}