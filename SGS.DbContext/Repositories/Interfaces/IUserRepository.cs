using SGS.DbContext.Repositories.BaseRepository;
using SGS.Domain.Entities;
using SGS.Domain.Enums;

namespace SGS.DbContext.Repositories.Interfaces;

public interface IUserRepository : IBaseRepository<User>
{
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<IEnumerable<User>> GetByRoleAsync(UserRole role, CancellationToken cancellationToken = default);
    Task AddUserAsync(User user, CancellationToken cancellationToken = default);
}