using SGS.Domain.Common;
using System.ComponentModel.DataAnnotations;

namespace SGS.Domain.Entities;

public class Role : BaseEntity
{
    [Required]
    public string RoleName { get; set; } = null!;

    public ICollection<User> Users { get; set; } = new List<User>();
}