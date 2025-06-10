using Microsoft.EntityFrameworkCore;
using SGS.Domain.Entities;
using SGS.Domain.Enums;
using SGS.Infrastructure.Data;
using SGS.Infrastructure.Repositories.BaseRepository;
using SGS.Infrastructure.Repositories.Interfaces;

namespace SGS.Infrastructure.Repositories;

public class UserRepository(AppDbContext context)
    : BaseRepository<User>(context), IUserRepository
{
    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await Context.Users
            .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
    }

    public async Task<IEnumerable<User>> GetByRoleAsync(UserRole role, CancellationToken cancellationToken = default)
    {
        return await Context.Users
            .Where(u => u.Role == role)
            .ToListAsync(cancellationToken);
    }
    
    public async Task AddUserAsync(User user, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(user.Email))
            throw new ArgumentException("User must have an email");
            
        if (string.IsNullOrEmpty(user.FullName))
            throw new ArgumentException("User must have a full name");
            
        var existingUser = await GetByEmailAsync(user.Email, cancellationToken);
        if (existingUser != null)
            throw new InvalidOperationException($"Email {user.Email} is already in use");
            
        await CreateAsync(user, cancellationToken);
        switch (user.Role)
        {
            case UserRole.Student:
                var student = new Student
                {
                    Id = Guid.NewGuid(),
                    UserId = user.Id,
                    User = user
                };
                await Context.Students.AddAsync(student, cancellationToken);
                break;
            
            case UserRole.Teacher:
                var teacher = new Teacher
                {
                    Id = Guid.NewGuid(),
                    UserId = user.Id,
                    User = user
                };
                await Context.Teachers.AddAsync(teacher, cancellationToken);
                break;
            
            case UserRole.Admin:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(user.Role), user.Role, "Invalid user role");
        }
    }
}