using SGS.Application.DTOs;
using SGS.Application.Models;

namespace SGS.Application.Interfaces;

public interface IUserService
{
    Task<Result<UserDto?>> GetUserByEmailAsync(string email, CancellationToken cancellationToken = default);
    
    Task<Result<IEnumerable<UserDto>>> GetTeachersAsync(CancellationToken cancellationToken = default);
    
    Task<Result<IEnumerable<UserDto>>> GetStudentsAsync(CancellationToken cancellationToken = default);
    
    Task<Result<IEnumerable<UserDto>?>> GetStudentsByGroupAsync(Guid groupId, CancellationToken cancellationToken = default);
    
    Task<Result<UserDto?>> GetUserByIdAsync(Guid id, CancellationToken cancellationToken = default);
}