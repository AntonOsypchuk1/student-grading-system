using Microsoft.EntityFrameworkCore;
using SGS.Domain.Entities;
using SGS.Domain.Enums;
using SGS.Infrastructure.Data;
using SGS.Infrastructure.Repositories.BaseRepository;
using SGS.Infrastructure.Repositories.Interfaces;

namespace SGS.Infrastructure.Repositories;

public class UserRepository(AppDbContext context) : BaseRepository<User>(context), IUserRepository
{
    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var users = await Context.Users.ToListAsync(cancellationToken);
        return await Context.Users
            .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
    }

    public async Task<IEnumerable<User>> GetByRoleAsync(UserRole role, CancellationToken cancellationToken = default)
    {
        return await Context.Users
            .Where(u => u.Role == role)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<User>> GetStudentsByGroupAsync(Guid groupId, CancellationToken cancellationToken = default)
    {
        return await Context.Users
            .Where(u => Context.Students.Any(s => s.UserId == u.Id && s.GroupId == groupId))
            .ToListAsync(cancellationToken);
    }
}