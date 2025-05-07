using AutoMapper;
using Microsoft.Extensions.Logging;
using SGS.Application.DTOs;
using SGS.Application.Helpers;
using SGS.Application.Interfaces;
using SGS.Infrastructure.Repositories.UnitOfWork;

namespace SGS.Application.Services.Course;

public class CourseService(
    IUnitOfWork unitOfWork,
    ILogger<CourseService> logger,
    IMapper mapper)
    : BaseService(unitOfWork, logger, mapper), ICourseService
{
    /*
     TODO: May be used to lookup teacher/student associations
     private readonly IUserRepository _userRepository = unitOfWork.Users;
    */

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

                await UnitOfWork.Courses.CreateAsync(course, cancellationToken);
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
        // For simplicity, we assume that the student is associated with one or more groups,
        // and groups are associated with courses via a many-to-many relationship.
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
}