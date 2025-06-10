using AutoMapper;
using Microsoft.Extensions.Logging;
using SGS.Application.DTOs;
using SGS.Application.Interfaces;
using SGS.Application.Models;
using SGS.Domain.Entities;
using SGS.Domain.Enums;
using SGS.Infrastructure.Repositories.UnitOfWork;

namespace SGS.Application.Services.User;

public class StudentsGroupService(
    IUnitOfWork unitOfWork,
    ILogger<UserService> logger,
    IMapper mapper) 
    : BaseService(unitOfWork, logger, mapper), IStudentsGroupService
{
    public async Task<Result<bool>> AssignUserToGroupAsync(Guid userId, Guid groupId, CancellationToken cancellationToken = default)
    {
        return await ExecuteInTransactionAsync(async () =>
        {
            Logger.LogInformation("Assigning user {UserId} to group {GroupId}", userId, groupId);

            try
            {
                var user = await UnitOfWork.Users.GetByIdAsync(userId, cancellationToken);
                if (user == null)
                {
                    Logger.LogWarning("User not found with ID: {UserId}", userId);
                    return Result<bool>.Failure($"User with ID {userId} not found.");
                }

                var group = await UnitOfWork.StudentsGroups.GetByIdAsync(groupId, cancellationToken);
                if (group == null)
                {
                    Logger.LogWarning("Group not found with ID: {GroupId}", groupId);
                    return Result<bool>.Failure($"Group with ID {groupId} not found.");
                }

                switch (user.Role)
                {
                    case UserRole.Student:
                    {
                        var existingGroup = await UnitOfWork.StudentsGroups.GetByStudentIdAsync(userId, cancellationToken);
                        if (existingGroup != null)
                        {
                            if (existingGroup.Id == groupId)
                            {
                                Logger.LogInformation("Student {UserId} is already assigned to group {GroupId}", userId, groupId);
                                return Result<bool>.Success(true);
                            }

                            Logger.LogInformation(
                                "Removing student {UserId} from existing group {GroupId} before assigning to new group",
                                userId, existingGroup.Id);
                            await UnitOfWork.StudentsGroups.RemoveStudentFromGroupAsync(userId, existingGroup.Id, cancellationToken);
                        }

                        await UnitOfWork.StudentsGroups.AddStudentToGroupAsync(groupId, userId, cancellationToken);
                        break;

                    }
                    case UserRole.Teacher:
                    // TODO:
                    // {
                    //     var teacherGroups = await UnitOfWork.StudentsGroups.GetByTeacherIdAsync(userId, cancellationToken);
                    //     if (teacherGroups.Any(g => g.Id == groupId))
                    //     {
                    //         Logger.LogInformation("Teacher {UserId} is already assigned to group {GroupId}", userId, groupId);
                    //         return Result<bool>.Success(true);
                    //     }
                    //
                    //     await UnitOfWork.StudentsGroups.AddTeacherToGroupAsync(groupId, userId, cancellationToken);
                    //     break;
                    // }
                    case UserRole.Admin:
                    default:
                        Logger.LogWarning("User with ID {UserId} has an invalid role {Role} for group assignment",
                            userId, user.Role);
                        return Result<bool>.Failure($"Users with role {user.Role} cannot be assigned to groups.");
                }

                await UnitOfWork.SaveChangesAsync(cancellationToken);
                Logger.LogInformation("Successfully assigned user {UserId} to group {GroupId}", userId, groupId);
                return Result<bool>.Success(true);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error assigning user {UserId} to group {GroupId}", userId, groupId);
                return Result<bool>.Failure($"Failed to assign user to group: {ex.Message}");
            }
        }, cancellationToken);
    }

    public async Task<Result<bool>> RemoveUserFromGroupAsync(Guid userId, Guid groupId, CancellationToken cancellationToken = default)
    {
        Logger.LogInformation("Removing user {UserId} from group {GroupId}", userId, groupId);

        try
        {
            var user = await UnitOfWork.Users.GetByIdAsync(userId, cancellationToken);
            if (user == null)
            {
                Logger.LogWarning("User not found with ID: {UserId}", userId);
                return Result<bool>.Failure("User not found");
            }

            var group = await UnitOfWork.StudentsGroups.GetByIdAsync(groupId, cancellationToken);
            if (group == null)
            {
                Logger.LogWarning("Group not found with ID: {GroupId}", groupId);
                return Result<bool>.Failure("Group not found");
            }

            switch (user.Role)
            {
                case UserRole.Student:
                    await UnitOfWork.StudentsGroups.RemoveStudentFromGroupAsync(userId, groupId, cancellationToken);
                    break;
                case UserRole.Teacher:
                    // TODO:
                    // await UnitOfWork.StudentsGroups.RemoveTeacherFromGroupAsync(userId, groupId, cancellationToken);
                    // break;
                case UserRole.Admin:
                default:
                    Logger.LogWarning("User with ID {UserId} has an invalid role {Role} for group removal",
                        userId, user.Role);
                    return Result<bool>.Failure($"Users with role {user.Role} cannot be removed from groups.");
            }

            await UnitOfWork.SaveChangesAsync(cancellationToken);
            Logger.LogInformation("Successfully removed user {UserId} from group {GroupId}", userId, groupId);
            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error removing user {UserId} from group {GroupId}", userId, groupId);
            return Result<bool>.Failure($"Failed to remove user from group: {ex.Message}");
        }
    }

    public async Task<Result<GroupDto?>> GetByGroupByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        Logger.LogInformation("Retrieving students group with name: {GroupName}", name);

        try
        {
            var group = await UnitOfWork.StudentsGroups.GetByGroupByNameAsync(name, cancellationToken);

            if (group == null)
            {
                Logger.LogWarning("Group not found with name: {GroupName}", name);
                return Result<GroupDto?>.Success(null);
            }
            
            Logger.LogInformation("Successfully retrieved group with name: {GroupName}", name);
            return Result<GroupDto?>.Success(Mapper.Map<GroupDto>(group));
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error retrieving group with name: {GroupName}", name);
            return Result<GroupDto?>.Failure($"Failed to retrieve user: {ex.Message}");
        }
    }

    public async Task<Result<IEnumerable<GroupDto>>> GetAllGroupsAsync(CancellationToken cancellationToken = default)
    {
        Logger.LogInformation("Retrieving all student groups");

        try
        {
            var groups = await UnitOfWork.StudentsGroups.GetAllAsync(cancellationToken);
            Logger.LogInformation("Successfully retrieved {Count} student groups", groups.Count());

            return Result<IEnumerable<GroupDto>>.Success(Mapper.Map<IEnumerable<GroupDto>>(groups));
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error retrieving all student groups");
            return Result<IEnumerable<GroupDto>>.Failure($"Failed to retrieve student groups: {ex.Message}");
        }
    }
    
    public async Task<Result<IEnumerable<GroupDto>>> GetGroupByCourseIdAsync(Guid courseId, CancellationToken cancellationToken = default)
    {
        Logger.LogInformation("Retrieving groups for course with ID: {CourseId}", courseId);

        try
        {
            var group = await UnitOfWork.StudentsGroups.GetByCourseIdAsync(courseId, cancellationToken);
            
            if (!group.Any())
            {
                Logger.LogWarning("No groups found for course with ID: {CourseId}", courseId);
                return Result<IEnumerable<GroupDto>>.Success([]);
            }
            
            Logger.LogInformation("Successfully retrieved group for student with ID: {CourseId}", courseId);
            return Result<IEnumerable<GroupDto>>.Success(Mapper.Map<IEnumerable<GroupDto>>(group));
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error retrieving groups for course with ID: {CourseId}", courseId);
            return Result<IEnumerable<GroupDto>>.Failure($"Failed to retrieve group for course: {ex.Message}");
        }
    }

    public async Task<Result<GroupDto?>> GetGroupByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        Logger.LogInformation("Retrieving student group with ID: {GroupId}", id);

        try
        {
            var group = await UnitOfWork.StudentsGroups.GetByIdAsync(id, cancellationToken);

            if (group == null)
            {
                Logger.LogWarning("Student group not found with ID: {GroupId}", id);
                return Result<GroupDto?>.Success(null);
            }
            
            Logger.LogInformation("Successfully retrieved student group with ID: {GroupId}", id);
            return Result<GroupDto?>.Success(Mapper.Map<GroupDto>(group));
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error retrieving student group with ID: {GroupId}", id);
            return Result<GroupDto?>.Failure($"Failed to retrieve student group: {ex.Message}");
        }
    }

    public async Task<Result<GroupDto>> CreateGroupAsync(CreateStudentsGroupDto
        groupDto, CancellationToken cancellationToken = default)
    {
        return await ExecuteInTransactionAsync(async () =>
        {
            Logger.LogInformation("Creating new student group with name: {GroupName}", groupDto.Name);

            try
            {
                // Check if teacher exists if TeacherId is provided
                // if (groupDto.TeacherId.HasValue)
                // {
                //     var teacher = await UnitOfWork.Users.GetByIdAsync(groupDto.TeacherId.Value, cancellationToken);
                //     if (teacher == null)
                //     {
                //         Logger.LogWarning("Teacher not found with ID: {TeacherId}", groupDto.TeacherId.Value);
                //         return Result<GroupDto>.Failure($"Teacher with ID {groupDto.TeacherId.Value} not found.");
                //     }
                // }

                var group = Mapper.Map<StudentsGroup>(groupDto);
                // group.CreatedAt = DateTime.UtcNow;

                await UnitOfWork.StudentsGroups.CreateAsync(group, cancellationToken);

                Logger.LogInformation("Successfully created student group with ID: {GroupId}", group.Id);
                return Result<GroupDto>.Success(Mapper.Map<GroupDto>(group));
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error creating student group");
                return Result<GroupDto>.Failure($"Failed to create student group: {ex.Message}");
            }
        }, cancellationToken);
    }

    public async Task<Result<GroupDto>> UpdateGroupAsync(Guid id, GroupDto groupDto, CancellationToken cancellationToken = default)
    {
        return await ExecuteInTransactionAsync(async () =>
        {
            Logger.LogInformation("Updating student group with ID: {GroupId}", id);

            try
            {
                var existingGroup = await UnitOfWork.StudentsGroups.GetByIdAsync(id, cancellationToken);
                if (existingGroup == null)
                {
                    Logger.LogWarning("Student group not found with ID: {GroupId}", id);
                    return Result<GroupDto>.Failure($"Student group with ID {id} not found.");
                }

                // Check if teacher exists if TeacherId is provided
                if (groupDto.TeacherId.HasValue)
                {
                    var teacher = await UnitOfWork.Users.GetByIdAsync(groupDto.TeacherId.Value, cancellationToken);
                    if (teacher == null)
                    {
                        Logger.LogWarning("Teacher not found with ID: {TeacherId}", groupDto.TeacherId.Value);
                        return Result<GroupDto>.Failure($"Teacher with ID {groupDto.TeacherId.Value} not found.");
                    }
                }

                // Update properties
                existingGroup.Name = groupDto.Name;
                existingGroup.Description = groupDto.Description;
                existingGroup.Year = groupDto.Year;
                // existingGroup.TeacherId = groupDto.TeacherId;
                existingGroup.UpdatedAt = DateTime.UtcNow;

                UnitOfWork.StudentsGroups.Update(existingGroup);
                await UnitOfWork.SaveChangesAsync(cancellationToken);

                Logger.LogInformation("Successfully updated student group with ID: {GroupId}", id);
                return Result<GroupDto>.Success(Mapper.Map<GroupDto>(existingGroup));
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error updating student group with ID: {GroupId}", id);
                return Result<GroupDto>.Failure($"Failed to update student group: {ex.Message}");
            }
        }, cancellationToken);
    }

    public async Task<Result<bool>> DeleteGroupAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await ExecuteInTransactionAsync(async () =>
        {
            Logger.LogInformation("Deleting student group with ID: {GroupId}", id);

            try
            {
                var group = await UnitOfWork.StudentsGroups.GetByIdAsync(id, cancellationToken);
                if (group == null)
                {
                    Logger.LogWarning("Student group not found with ID: {GroupId}", id);
                    return Result<bool>.Failure($"Student group with ID {id} not found.");
                }

                var studentsInGroup = group.Students;
                if (studentsInGroup.Count != 0)
                {
                    Logger.LogWarning("Cannot delete group with ID: {GroupId} because it has {Count} students",
                        id, studentsInGroup.Count);
                    return Result<bool>.Failure(
                        "Cannot delete a group that has students. Please reassign or remove students first.");
                }

                UnitOfWork.StudentsGroups.Delete(group);
                await UnitOfWork.SaveChangesAsync(cancellationToken);

                Logger.LogInformation("Successfully deleted student group with ID: {GroupId}", id);
                return Result<bool>.Success(true);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error deleting student group with ID: {GroupId}", id);
                return Result<bool>.Failure($"Failed to delete student group: {ex.Message}");
            }
        }, cancellationToken);
    }

    public async Task<Result<GroupDto?>> GetGroupByStudentIdAsync(Guid studentId, CancellationToken cancellationToken = default)
    {
        Logger.LogInformation("Retrieving group for student with ID: {StudentId}", studentId);

        try
        {
            var group = await UnitOfWork.StudentsGroups.GetByStudentIdAsync(studentId, cancellationToken);
            
            if (group == null)
            {
                Logger.LogWarning("No group found for student with ID: {StudentId}", studentId);
                return Result<GroupDto?>.Success(null);
            }
            
            Logger.LogInformation("Successfully retrieved group for student with ID: {StudentId}", studentId);
            return Result<GroupDto?>.Success(Mapper.Map<GroupDto>(group));
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error retrieving group for student with ID: {StudentId}", studentId);
            return Result<GroupDto?>.Failure($"Failed to retrieve group for student: {ex.Message}");
        }
    }

    public async Task<Result<IEnumerable<UserDto>?>> GetStudentsByGroupAsync(Guid groupId,
        CancellationToken cancellationToken = default)
    {
        Logger.LogInformation("Retrieving students for group: {GroupId}", groupId);

        try
        {
            var group = await UnitOfWork.StudentsGroups.GetByIdAsync(groupId, cancellationToken);
            var students = group?.Students;
            if (students == null || students.Count == 0)
            {
                Logger.LogWarning("No students found for group: {GroupId}", groupId);
                return Result<IEnumerable<UserDto>?>.Success(null);
            }
            Logger.LogInformation("Successfully retrieved {Count} students for group: {GroupId}",
                students.Count, groupId);

            return Result<IEnumerable<UserDto>?>.Success(Mapper.Map<IEnumerable<UserDto>>(students));
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error retrieving students for group: {GroupId}", groupId);
            return Result<IEnumerable<UserDto>?>.Failure($"Failed to retrieve students: {ex.Message}");
        }
    }
}