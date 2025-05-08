using SGS.Application.DTOs;
using SGS.Application.Models;

namespace SGS.Application.Interfaces;

public interface IAuthService
{
    Task<Result<AuthResponseDto>> LoginAsync(string email, string password, CancellationToken cancellationToken = default);
    
    Task<Result<UserDto>> RegisterUserAsync(RegisterUserCommand command, CancellationToken cancellationToken = default);
}