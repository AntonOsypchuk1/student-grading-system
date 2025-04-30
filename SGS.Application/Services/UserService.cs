using AutoMapper;
using Microsoft.Extensions.Logging;
using SGS.Application.DTOs;
using SGS.Application.Interfaces;
using SGS.Domain.Enums;
using SGS.Infrastructure.Data;
using SGS.Infrastructure.Repositories.Interfaces;
using SGS.Infrastructure.Repositories.UnitOfWork;

namespace SGS.Application.Services;

public class UserService(
    IUnitOfWork unitOfWork,
    ILogger<UserService> logger,
    IMapper mapper)
    : IUserService
{
    private readonly IUserRepository _userRepository = unitOfWork.Users;
    
    public async Task<UserDto?> GetUserByEmailAsync(string email,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Retrieving user by email: {Email}", email);

        try
        {
            var user = await _userRepository.GetByEmailAsync(email, cancellationToken);
            
            if (user == null)
            {
                logger.LogWarning("User not found with email: {Email}", email);
                return null;
            }

            logger.LogInformation("Successfully retrieved user with email: {Email}", email);
            return mapper.Map<UserDto>(user);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving user by email: {Email}", email);
            throw;
        }
    }

    public async Task<IEnumerable<UserDto>> GetTeachersAsync(
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Retrieving all teachers");

        try
        {
            var teachers = await _userRepository.GetByRoleAsync(UserRole.Teacher, cancellationToken);
            logger.LogInformation("Successfully retrieved {Count} teachers", teachers.Count());

            return mapper.Map<IEnumerable<UserDto>>(teachers);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving teachers");
            throw;
        }
    }

    public async Task<IEnumerable<UserDto>> GetStudentsAsync(
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Retrieving all students");

        try
        {
            var students = await _userRepository.GetByRoleAsync(UserRole.Student, cancellationToken);
            logger.LogInformation("Successfully retrieved {Count} students", students.Count());

            return mapper.Map<IEnumerable<UserDto>>(students);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving students");
            throw;
        }
    }

    public async Task<IEnumerable<UserDto>?> GetStudentsByGroupAsync(Guid groupId,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Retrieving students for group: {GroupId}", groupId);

        try
        {
            var groupStudents = await _userRepository.GetStudentsByGroupAsync(groupId, cancellationToken);
            logger.LogInformation("Successfully retrieved {Count} students for group: {GroupId}",
                groupStudents.Count(), groupId);

            return mapper.Map<IEnumerable<UserDto>>(groupStudents);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving students for group: {GroupId}", groupId);
            throw;
        }
    }
}