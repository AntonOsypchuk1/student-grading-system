using SGS.Application.DTOs;

namespace SGS.Application.Interfaces;

public interface IUserService
{
    Task<UserDto> RegisterUserAsync(RegisterUserCommand command, CancellationToken cancellationToken = default);
    Task<UserDto?> GetUserByEmailAsync(string email, CancellationToken cancellationToken = default);
}