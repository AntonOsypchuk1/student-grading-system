using AutoMapper;
using Microsoft.Extensions.Logging;
using SGS.Application.DTOs;
using SGS.Application.Interfaces;
using SGS.Application.Models;
using SGS.Domain.Entities;
using SGS.Domain.Enums;
using SGS.Infrastructure.Repositories.UnitOfWork;

namespace SGS.Application.Services.Course;

public class CourseService(
    IUnitOfWork unitOfWork,
    ILogger<CourseService> logger,
    IMapper mapper)
    : BaseService(unitOfWork, logger, mapper), ICourseService
{
    public async Task<Result<CourseDto>> CreateCourseAsync(CreateCourseCommand command,
        CancellationToken cancellationToken = default)
    {
        return await ExecuteInTransactionAsync(async () =>
        {
            Logger.LogInformation("Creating course with title: {Title}", command.Title);
            try
            {
                var course = new Domain.Entities.Course
                {
                    Id = Guid.NewGuid(),
                    Title = command.Title,
                    Description = command.Description
                };

                await UnitOfWork.Courses.AddCourseAsync(course, cancellationToken);
                Logger.LogInformation("Successfully created course with ID: {CourseId}", course.Id);

                return Result<CourseDto>.Success(Mapper.Map<CourseDto>(course));
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error creating course with title: {Title}", command.Title);
                return Result<CourseDto>.Failure($"Failed to create course: {ex.Message}");
            }
        }, cancellationToken);
    }

    public async Task<Result<IEnumerable<CourseDto>>> GetAllCoursesAsync(CancellationToken cancellationToken = default)
    {
        Logger.LogInformation("Retrieving all courses");
        try
        {
            var courses = await UnitOfWork.Courses.GetAllAsync(cancellationToken);
            Logger.LogInformation("Successfully retrieved {Count} courses", courses.Count());

            return Result<IEnumerable<CourseDto>>.Success(Mapper.Map<IEnumerable<CourseDto>>(courses));
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error retrieving all courses");
            return Result<IEnumerable<CourseDto>>.Failure($"Failed to retrieve courses: {ex.Message}");
        }
    }

    public async Task<Result<CourseDto?>> GetCourseByIdAsync(Guid courseId, CancellationToken cancellationToken = default)
    {
        Logger.LogInformation("Retrieving course with ID: {CourseId}", courseId);

        try
        {
            var course = await UnitOfWork.Courses.GetByIdAsync(courseId, cancellationToken);

            if (course == null)
            {
                Logger.LogWarning("Course not found with ID: {CourseId}", courseId);
                return Result<CourseDto?>.Success(null);
            }

            Logger.LogInformation("Successfully retrieved course with ID: {CourseId}", courseId);
            return Result<CourseDto?>.Success(Mapper.Map<CourseDto>(course));
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error retrieving course with ID: {CourseId}", courseId);
            return Result<CourseDto?>.Failure($"Failed to retrieve course: {ex.Message}");
        }
    }

    public async Task<Result<IEnumerable<CourseDto>>> GetCoursesByTeacherAsync(Guid teacherId,
        CancellationToken cancellationToken = default)
    {
        Logger.LogInformation("Retrieving courses for teacher with ID: {TeacherId}", teacherId);

        try
        {
            var courses = await UnitOfWork.Courses.GetByTeacherIdAsync(teacherId, cancellationToken);
            Logger.LogInformation("Successfully retrieved {Count} courses for teacher: {TeacherId}", 
                courses.Count(), teacherId);

            return Result<IEnumerable<CourseDto>>.Success(Mapper.Map<IEnumerable<CourseDto>>(courses));
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error retrieving courses for teacher with ID: {TeacherId}", teacherId);
            return Result<IEnumerable<CourseDto>>.Failure($"Failed to retrieve courses: {ex.Message}");
        }
    }

    public async Task<Result<IEnumerable<CourseDto>>> GetCoursesByStudentAsync(Guid studentId,
        CancellationToken cancellationToken = default)
    {
        Logger.LogInformation("Retrieving courses for student with ID: {StudentId}", studentId);

        try
        {
            var courses = await UnitOfWork.Courses.GetByStudentIdAsync(studentId, cancellationToken);
            Logger.LogInformation("Successfully retrieved {Count} courses for student: {StudentId}", 
                courses.Count(), studentId);

            return Result<IEnumerable<CourseDto>>.Success(Mapper.Map<IEnumerable<CourseDto>>(courses));
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error retrieving courses for student with ID: {StudentId}", studentId);
            return Result<IEnumerable<CourseDto>>.Failure($"Failed to retrieve courses: {ex.Message}");
        }
    }

    public async Task<Result<bool>> AssignTeacherToCourseAsync(Guid teacherId, Guid courseId, CancellationToken cancellationToken = default)
    {
        return await ExecuteInTransactionAsync(async () =>
        {
            Logger.LogInformation("Assigning teacher {TeacherId} to course {CourseId}", teacherId, courseId);

            try
            {
                var teacher = await UnitOfWork.Users.GetByIdAsync(teacherId, cancellationToken);
                if (teacher == null)
                {
                    var user = await UnitOfWork.Users.GetByIdAsync(teacherId, cancellationToken);
                    if (user == null)
                    {
                        Logger.LogWarning("User not found with ID: {UserId}", teacherId);
                        return Result<bool>.Failure("User not found");
                    }

                    if (user.Role != UserRole.Teacher)
                    {
                        Logger.LogWarning("User with ID {UserId} is not a teacher", teacherId);
                        return Result<bool>.Failure("User is not a teacher");
                    }
                }

                await UnitOfWork.Courses.AddTeacherToCourseAsync(courseId, teacher.Id, cancellationToken);

                Logger.LogInformation("Successfully assigned teacher {TeacherId} to course {CourseId}", teacherId,
                    courseId);
                return Result<bool>.Success(true);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error assigning teacher {TeacherId} to course {CourseId}", teacherId, courseId);
                return Result<bool>.Failure($"Failed to assign teacher to course: {ex.Message}");
            }
        }, cancellationToken);
    }

    // public async Task<Result<bool>> RemoveTeacherFromCourseAsync(Guid teacherId, Guid courseId, CancellationToken cancellationToken = default)
    // {
    //     return await ExecuteInTransactionAsync(async () =>
    //     {
    //         Logger.LogInformation("Removing teacher {TeacherId} from course {CourseId}", teacherId, courseId);
    //
    //         try
    //         {
    //             // Verify teacher exists
    //             var teacher = await UnitOfWork.Teachers.GetByUserIdAsync(teacherId, cancellationToken);
    //             if (teacher == null)
    //             {
    //                 Logger.LogWarning("Teacher not found with ID: {TeacherId}", teacherId);
    //                 return Result<bool>.Failure("Teacher not found");
    //             }
    //
    //             // Verify course exists
    //             var course = await UnitOfWork.Courses.GetByIdAsync(courseId, cancellationToken);
    //             if (course == null)
    //             {
    //                 Logger.LogWarning("Course not found with ID: {CourseId}", courseId);
    //                 return Result<bool>.Failure("Course not found");
    //             }
    //
    //             // Check if teacher is assigned to course
    //             var teacherCourses = await UnitOfWork.Courses.GetByTeacherIdAsync(teacherId, cancellationToken);
    //             if (!teacherCourses.Any(c => c.Id == courseId))
    //             {
    //                 Logger.LogInformation("Teacher {TeacherId} is not assigned to course {CourseId}", teacherId,
    //                     courseId);
    //                 return Result<bool>.Success(true);
    //             }
    //
    //             // Remove teacher from course
    //             await UnitOfWork.Courses.RemoveTeacherFromCourseAsync(courseId, teacher.Id, cancellationToken);
    //
    //             Logger.LogInformation("Successfully removed teacher {TeacherId} from course {CourseId}", teacherId,
    //                 courseId);
    //             return Result<bool>.Success(true);
    //         }
    //         catch (Exception ex)
    //         {
    //             Logger.LogError(ex, "Error removing teacher {TeacherId} from course {CourseId}", teacherId, courseId);
    //             return Result<bool>.Failure($"Failed to remove teacher from course: {ex.Message}");
    //         }
    //     }, cancellationToken);
    // }

    public async Task<Result<bool>> AssignGroupToCourseAsync(Guid groupId, Guid courseId, CancellationToken cancellationToken = default)
    {
        return await ExecuteInTransactionAsync(async () =>
        {
            Logger.LogInformation("Assigning group {GroupId} to course {CourseId}", groupId, courseId);

            try
            {
                // Verify group exists
                var group = await UnitOfWork.StudentsGroups.GetByIdAsync(groupId, cancellationToken);
                if (group == null)
                {
                    Logger.LogWarning("Group not found with ID: {GroupId}", groupId);
                    return Result<bool>.Failure("Group not found");
                }

                // var course = await UnitOfWork.Courses.GetByIdAsync(courseId, cancellationToken);
                // if (course == null)
                // {
                //     Logger.LogWarning("Course not found with ID: {CourseId}", courseId);
                //     return Result<bool>.Failure("Course not found");
                // }

                // var groupCourses = await UnitOfWork.StudentsGroups.GetByCourseIdAsync(courseId, cancellationToken);
                // if (groupCourses.Any(g => g.Id == groupId))
                // {
                //     Logger.LogInformation("Group {GroupId} is already assigned to course {CourseId}", groupId, courseId);
                //     return Result<bool>.Success(true);
                // }

                await UnitOfWork.Courses.AddStudentsGroupToCourseAsync(courseId, groupId, cancellationToken);
            
                Logger.LogInformation("Successfully assigned group {GroupId} to course {CourseId}", groupId, courseId);
                return Result<bool>.Success(true);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error assigning group {GroupId} to course {CourseId}", groupId, courseId);
                return Result<bool>.Failure($"Failed to assign group to course: {ex.Message}");
            }

        }, cancellationToken);
    }

    // public async Task<Result<bool>> RemoveGroupFromCourseAsync(Guid groupId, Guid courseId, CancellationToken cancellationToken = default)
    // {
    //     Logger.LogInformation("Removing group {GroupId} from course {CourseId}", groupId, courseId);
    //     try
    //     {
    //         // Verify group exists
    //         var group = await UnitOfWork.StudentsGroups.GetByIdAsync(groupId, cancellationToken);
    //         if (group == null)
    //         {
    //             Logger.LogWarning("Group not found with ID: {GroupId}", groupId);
    //             return Result<bool>.Failure("Group not found");
    //         }
    //
    //         // Verify course exists
    //         var course = await UnitOfWork.Courses.GetByIdAsync(courseId, cancellationToken);
    //         if (course == null)
    //         {
    //             Logger.LogWarning("Course not found with ID: {CourseId}", courseId);
    //             return Result<bool>.Failure("Course not found");
    //         }
    //
    //         // Check if group is assigned to course
    //         var groupCourses = await UnitOfWork.StudentsGroups.GetByCourseIdAsync(courseId, cancellationToken);
    //         if (!groupCourses.Any(g => g.Id == groupId))
    //         {
    //             Logger.LogInformation("Group {GroupId} is not assigned to course {CourseId}", groupId, courseId);
    //             return Result<bool>.Success(true);
    //         }
    //
    //         // Check if there are any assignments for this course and group
    //         var assignments = await UnitOfWork.Assignments.GetByCourseAndGroupIdAsync(courseId, groupId, cancellationToken);
    //         if (assignments.Any())
    //         {
    //             Logger.LogWarning("Cannot remove group {GroupId} from course {CourseId} because there are assignments", groupId, courseId);
    //             return Result<bool>.Failure("Cannot remove group from course because there are assignments");
    //         }
    //
    //         // Remove group from course
    //         await UnitOfWork.Courses.RemoveGroupFromCourseAsync(courseId, groupId, cancellationToken);
    //         
    //         Logger.LogInformation("Successfully removed group {GroupId} from course {CourseId}", groupId, courseId);
    //         return Result<bool>.Success(true);
    //     }
    //     catch (Exception ex)
    //     {
    //         Logger.LogError(ex, "Error removing group {GroupId} from course {CourseId}", groupId, courseId);
    //         return Result<bool>.Failure($"Failed to remove group from course: {ex.Message}");
    //     }
    // }
}