using SGS.Application.DTOs;

namespace SGS.Application.Interfaces;

public interface IAuthService
{
    Task<AuthResponseDto> LoginAsync(string email, string password, CancellationToken cancellationToken = default);
    
    Task<UserDto> RegisterUserAsync(RegisterUserCommand command, CancellationToken cancellationToken = default);
}