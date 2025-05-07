using AutoMapper;
using Microsoft.Extensions.Logging;
using SGS.Application.DTOs;
using SGS.Application.Helpers;
using SGS.Application.Interfaces;
using SGS.Domain.Enums;
using SGS.Infrastructure.Repositories.UnitOfWork;

namespace SGS.Application.Services.User;

public class UserService(
    IUnitOfWork unitOfWork,
    ILogger<UserService> logger,
    IMapper mapper)
    : BaseService(unitOfWork, logger, mapper), IUserService
{
    public async Task<Result<UserDto?>> GetUserByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        Logger.LogInformation("Retrieving user with ID: {UserId}", id);

        try
        {
            var user = await UnitOfWork.Users.GetByIdAsync(id, cancellationToken);

            if (user == null)
            {
                Logger.LogWarning("User not found with ID: {UserId}", id);
                return Result<UserDto?>.Success(null);
            }
            
            Logger.LogInformation("Successfully retrieved user with ID: {UserId}", id);
            return Result<UserDto?>.Success(Mapper.Map<UserDto>(user));
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error retrieving user with ID: {UserId}", id);
            return Result<UserDto?>.Failure($"Failed to retrieve user: {ex.Message}");
        }
    }
    
    public async Task<Result<UserDto?>> GetUserByEmailAsync(string email,
        CancellationToken cancellationToken = default)
    {
        Logger.LogInformation("Retrieving user by email: {Email}", email);

        try
        {
            var user = await UnitOfWork.Users.GetByEmailAsync(email, cancellationToken);
            
            if (user == null)
            {
                Logger.LogWarning("User not found with email: {Email}", email);
                return Result<UserDto?>.Success(null);
            }

            Logger.LogInformation("Successfully retrieved user with email: {Email}", email);
            return Result<UserDto?>.Success(Mapper.Map<UserDto>(user));
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error retrieving user by email: {Email}", email);
            return Result<UserDto?>.Failure($"Failed to retrieve user: {ex.Message}");
        }
    }

    public async Task<Result<IEnumerable<UserDto>>> GetTeachersAsync(
        CancellationToken cancellationToken = default)
    {
        Logger.LogInformation("Retrieving all teachers");

        try
        {
            var teachers = await UnitOfWork.Users.GetByRoleAsync(UserRole.Teacher, cancellationToken);
            Logger.LogInformation("Successfully retrieved {Count} teachers", teachers.Count());

            return Result<IEnumerable<UserDto>>.Success(Mapper.Map<IEnumerable<UserDto>>(teachers));
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error retrieving teachers");
            return Result<IEnumerable<UserDto>>.Failure($"Failed to retrieve teachers: {ex.Message}");
        }
    }

    public async Task<Result<IEnumerable<UserDto>>> GetStudentsAsync(
        CancellationToken cancellationToken = default)
    {
        Logger.LogInformation("Retrieving all students");

        try
        {
            var students = await UnitOfWork.Users.GetByRoleAsync(UserRole.Student, cancellationToken);
            Logger.LogInformation("Successfully retrieved {Count} students", students.Count());

            return Result<IEnumerable<UserDto>>.Success(Mapper.Map<IEnumerable<UserDto>>(students));
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error retrieving students");
            return Result<IEnumerable<UserDto>>.Failure($"Failed to retrieve students: {ex.Message}");
        }
    }

    public async Task<Result<IEnumerable<UserDto>?>> GetStudentsByGroupAsync(Guid groupId,
        CancellationToken cancellationToken = default)
    {
        Logger.LogInformation("Retrieving students for group: {GroupId}", groupId);

        try
        {
            var groupStudents = await UnitOfWork.Users.GetStudentsByGroupAsync(groupId, cancellationToken);
            Logger.LogInformation("Successfully retrieved {Count} students for group: {GroupId}",
                groupStudents.Count(), groupId);

            return Result<IEnumerable<UserDto>?>.Success(Mapper.Map<IEnumerable<UserDto>>(groupStudents));
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error retrieving students for group: {GroupId}", groupId);
            return Result<IEnumerable<UserDto>?>.Failure($"Failed to retrieve students: {ex.Message}");
        }
    }
}