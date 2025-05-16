namespace SGS.Application.DTOs;

public class AuthResponseDto
{
    public string Token { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; } // 
    public string Role { get; set; } = string.Empty; //
    public string FullName { get; set; } = string.Empty; //
    public UserDto User { get; set; } = null!;
}

public class LoginDto
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}