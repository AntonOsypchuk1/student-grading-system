using AutoMapper;
using Microsoft.Extensions.Logging;
using SGS.Application.DTOs;
using SGS.Application.Interfaces;
using SGS.Application.Services.Bases;
using SGS.Domain.Enums;
using SGS.Infrastructure.Data;
using SGS.Infrastructure.Repositories.Interfaces;
using SGS.Infrastructure.Repositories.UnitOfWork;

namespace SGS.Application.Services;

public class UserService(
    IUnitOfWork unitOfWork,
    ILogger<UserService> logger,
    IMapper mapper)
    : BaseService(unitOfWork, logger, mapper), IUserService
{
    public async Task<UserDto?> GetUserByEmailAsync(string email,
        CancellationToken cancellationToken = default)
    {
        Logger.LogInformation("Retrieving user by email: {Email}", email);

        try
        {
            var user = await UnitOfWork.Users.GetByEmailAsync(email, cancellationToken);
            
            if (user == null)
            {
                Logger.LogWarning("User not found with email: {Email}", email);
                return null;
            }

            Logger.LogInformation("Successfully retrieved user with email: {Email}", email);
            return Mapper.Map<UserDto>(user);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error retrieving user by email: {Email}", email);
            throw;
        }
    }

    public async Task<IEnumerable<UserDto>> GetTeachersAsync(
        CancellationToken cancellationToken = default)
    {
        Logger.LogInformation("Retrieving all teachers");

        try
        {
            var teachers = await UnitOfWork.Users.GetByRoleAsync(UserRole.Teacher, cancellationToken);
            Logger.LogInformation("Successfully retrieved {Count} teachers", teachers.Count());

            return Mapper.Map<IEnumerable<UserDto>>(teachers);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error retrieving teachers");
            throw;
        }
    }

    public async Task<IEnumerable<UserDto>> GetStudentsAsync(
        CancellationToken cancellationToken = default)
    {
        Logger.LogInformation("Retrieving all students");

        try
        {
            var students = await UnitOfWork.Users.GetByRoleAsync(UserRole.Student, cancellationToken);
            Logger.LogInformation("Successfully retrieved {Count} students", students.Count());

            return Mapper.Map<IEnumerable<UserDto>>(students);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error retrieving students");
            throw;
        }
    }

    public async Task<IEnumerable<UserDto>?> GetStudentsByGroupAsync(Guid groupId,
        CancellationToken cancellationToken = default)
    {
        Logger.LogInformation("Retrieving students for group: {GroupId}", groupId);

        try
        {
            var groupStudents = await UnitOfWork.Users.GetStudentsByGroupAsync(groupId, cancellationToken);
            Logger.LogInformation("Successfully retrieved {Count} students for group: {GroupId}",
                groupStudents.Count(), groupId);

            return Mapper.Map<IEnumerable<UserDto>>(groupStudents);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error retrieving students for group: {GroupId}", groupId);
            throw;
        }
    }
}