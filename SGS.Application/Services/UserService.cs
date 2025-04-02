using AutoMapper;
using SGS.Application.DTOs;
using SGS.Application.Interfaces;
using SGS.Domain.Entities;
using SGS.Domain.Enums;
using SGS.Infrastructure.Repositories.Interfaces;

namespace SGS.Application.Services;

public class UserService(IUserRepository userRepository, IMapper mapper) : IUserService
{
    public async Task<UserDto> RegisterUserAsync(RegisterUserCommand command, CancellationToken cancellationToken = default)
    {
        // Check if the user exists
        var existingUser = await userRepository.GetByEmailAsync(command.Email, cancellationToken);
        if (existingUser != null)
        {
            throw new Exception("User with this email already exists.");
        }
        
        if (!Enum.TryParse(command.Role, true, out UserRole role))
        {
            throw new Exception("Invalid role provided.");
        }
            
        // Create a new User domain entity
        var user = new User
        {
            Id = Guid.NewGuid(),
            FullName = command.FullName,
            Email = command.Email,
            PasswordHash = HashPassword(command.Password), // Implement a password hashing mechanism
            Role = role
        };

        await userRepository.CreateAsync(user, cancellationToken);
        return mapper.Map<UserDto>(user);
    }

    public async Task<UserDto?> GetUserByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var user = await userRepository.GetByEmailAsync(email, cancellationToken);
        return user == null ? null : mapper.Map<UserDto>(user);
    }

    // Dummy implementation for password hashing – replace with a secure one
    private string HashPassword(string password)
    {
        return $"hashed-{password}";
    }
}