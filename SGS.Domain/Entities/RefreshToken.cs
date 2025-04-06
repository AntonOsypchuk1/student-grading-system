using SGS.Domain.Common;

namespace SGS.Domain.Entities;

public class RefreshToken : BaseEntity
{
    public Guid UserId { get; set; }
    public string Token { get; set; } = string.Empty;
    public DateTime Expires { get; set; }
    public DateTime Created { get; set; }
    public DateTime? Revoked { get; set; }

    public bool IsActive => Revoked == null && DateTime.UtcNow <= Expires;
}