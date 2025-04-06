using System.ComponentModel.DataAnnotations;
using SGS.Domain.Common;

namespace SGS.Domain.Entities;

public class RefreshToken : BaseEntity
{
    public Guid UserId { get; set; }
    
    [Required]
    public string Token { get; set; } = string.Empty;
    
    [Required]
    public DateTime Expires { get; set; }
    
    [Required]
    public DateTime Created { get; set; }
    
    public DateTime? Revoked { get; set; }

    public bool IsActive => Revoked == null && DateTime.UtcNow <= Expires;
}