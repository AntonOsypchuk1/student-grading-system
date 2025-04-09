using SGS.Domain.Entities;
using SGS.Domain.Enums;
using SGS.Infrastructure.Repositories.BaseRepository;

namespace SGS.Infrastructure.Repositories.Interfaces;

public interface IUserRepository : IBaseRepository<User>
{
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    
    Task<IEnumerable<User>> GetByRoleAsync(UserRole role, CancellationToken cancellationToken = default);
    
    Task<IEnumerable<User>> GetStudentsByGroupAsync(Guid groupId, CancellationToken cancellationToken = default);
}