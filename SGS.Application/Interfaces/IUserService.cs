using SGS.Application.DTOs;

namespace SGS.Application.Interfaces;

public interface IUserService
{
    Task<UserDto?> GetUserByEmailAsync(string email, CancellationToken cancellationToken = default);
    
    Task<IEnumerable<UserDto>> GetTeachersAsync(CancellationToken cancellationToken = default);
    
    Task<IEnumerable<UserDto>> GetStudentsAsync(CancellationToken cancellationToken = default);
    
    Task<IEnumerable<UserDto>> GetStudentsByGroupAsync(Guid groupId, CancellationToken cancellationToken = default);
}