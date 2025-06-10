using AutoMapper;
using Microsoft.Extensions.Logging;
using SGS.Application.DTOs;
using SGS.Application.Interfaces;
using SGS.Application.Models;
using SGS.Domain.Enums;
using SGS.Infrastructure.Repositories.UnitOfWork;

namespace SGS.Application.Services.User;

public class UserService(
    IUnitOfWork unitOfWork,
    ILogger<UserService> logger,
    IMapper mapper)
    : BaseService(unitOfWork, logger, mapper), IUserService
{
    public async Task<Result<UserDto>> UpdateUserAsync(Guid id, UserDto updateUserDto,
        CancellationToken cancellationToken = default)
    {
        Logger.LogInformation("Updating user with ID: {UserId}", id);

        try
        {
            var existingUser = await UnitOfWork.Users.GetByIdAsync(id, cancellationToken);
            if (existingUser == null)
            {
                Logger.LogWarning("User not found with ID: {UserId}", id);
                return Result<UserDto>.Failure("User not found");
            }

            Mapper.Map(updateUserDto, existingUser);

            UnitOfWork.Users.Update(existingUser);

            if (existingUser.Role == UserRole.Student && updateUserDto.GroupId.HasValue)
            {
                var currentGroup = await UnitOfWork.StudentsGroups.GetByStudentIdAsync(id, cancellationToken);
                var newGroup =
                    await UnitOfWork.StudentsGroups.GetByIdAsync(updateUserDto.GroupId.Value, cancellationToken);

                if (newGroup == null)
                {
                    Logger.LogWarning("Group not found with ID: {GroupId}", updateUserDto.GroupId.Value);
                    return Result<UserDto>.Failure("Specified group not found");
                }

                if (currentGroup == null || currentGroup.Id != newGroup.Id)
                {
                    await UnitOfWork.StudentsGroups.AddStudentToGroupAsync(newGroup.Id, id, cancellationToken);
                }
            }

            Logger.LogInformation("Successfully updated user with ID: {UserId}", id);
            return Result<UserDto>.Success(Mapper.Map<UserDto>(updateUserDto));
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error updating user with ID: {UserId}", id);
            return Result<UserDto>.Failure($"Failed to update user: {ex.Message}");
        }
    }
    
    public async Task<Result<bool>> DeleteUserAsync(Guid id, CancellationToken cancellationToken = default)
    {
        Logger.LogInformation("Deleting user with ID: {UserId}", id);

        try
        {
            var user = await UnitOfWork.Users.GetByIdAsync(id, cancellationToken);
            if (user == null)
            {
                Logger.LogWarning("User not found with ID: {UserId}", id);
                return Result<bool>.Failure("User not found");
            }
        
            UnitOfWork.Users.Delete(user);
        
            Logger.LogInformation("Successfully deleted user with ID: {UserId}", id);
            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error deleting user with ID: {UserId}", id);
            return Result<bool>.Failure($"Failed to delete user: {ex.Message}");
        }
    }

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
            
            if (user is null)
            {
                Logger.LogWarning("User not found with email: {Email}", email);
                return Result<UserDto?>.Success(null);
            }

            var result = new UserDto
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                Role = user.Role.ToString(),
            };

            if (user.Role == UserRole.Student)
            {
                var group = await UnitOfWork.StudentsGroups.GetByStudentIdAsync(user.Id, cancellationToken);
                
                if (group is null)
                {
                    Logger.LogWarning("Group for user not found with Id: {UserId}", user.Id);
                    return Result<UserDto?>.Success(null);
                }

                result.GroupId = group.Id;
            }

            Logger.LogInformation("Successfully retrieved user with email: {Email}", email);
            return Result<UserDto?>.Success(result);
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
}