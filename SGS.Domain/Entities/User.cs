using System.ComponentModel.DataAnnotations;
using SGS.Domain.Common;
using SGS.Domain.Enums;

namespace SGS.Domain.Entities;

public class User : BaseEntity
{
    [Required, MaxLength(50)]
    public string Login { get; set; } = null!;
    
    [Required, EmailAddress]
    public string Email { get; set; } = null!;
    
    [Required, MaxLength(100)]
    public string FullName { get; set; } = null!;
    
    [Required]
    public string PasswordHash { get; set; } = null!;

    [Required]
    public UserRole Role { get; set; }
}