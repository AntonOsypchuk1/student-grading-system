using AutoMapper;
using Microsoft.Extensions.Logging;
using SGS.Application.DTOs;
using SGS.Application.Interfaces;
using SGS.Application.Services.Bases;
using SGS.Domain.Entities;
using SGS.Infrastructure.Data;
using SGS.Infrastructure.Repositories.Interfaces;
using SGS.Infrastructure.Repositories.UnitOfWork;

namespace SGS.Application.Services;

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

    public async Task<CourseDto> CreateCourseAsync(CreateCourseCommand command,
        CancellationToken cancellationToken = default)
    {
        return await ExecuteInTransactionAsync(async () =>
        {
            Logger.LogInformation("Creating course with title: {Title}", command.Title);
            try
            {
                var course = new Course
                {
                    Id = Guid.NewGuid(),
                    Title = command.Title,
                    Description = command.Description
                };

                await UnitOfWork.Courses.CreateAsync(course, cancellationToken);
                Logger.LogInformation("Successfully created course with ID: {CourseId}", course.Id);

                return Mapper.Map<CourseDto>(course);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error creating course with title: {Title}", command.Title);
                throw;
            }
        }, cancellationToken);
    }

    public async Task<CourseDto?> GetCourseByIdAsync(Guid courseId, CancellationToken cancellationToken = default)
    {
        Logger.LogInformation("Retrieving course with ID: {CourseId}", courseId);

        try
        {
            var course = await UnitOfWork.Courses.GetByIdAsync(courseId, cancellationToken);

            if (course == null)
            {
                Logger.LogWarning("Course not found with ID: {CourseId}", courseId);
                return null;
            }

            Logger.LogInformation("Successfully retrieved course with ID: {CourseId}", courseId);
            return Mapper.Map<CourseDto>(course);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error retrieving course with ID: {CourseId}", courseId);
            throw;
        }
    }

    public async Task<IEnumerable<CourseDto>> GetCoursesByTeacherAsync(Guid teacherId,
        CancellationToken cancellationToken = default)
    {
        Logger.LogInformation("Retrieving courses for teacher with ID: {TeacherId}", teacherId);

        try
        {
            var courses = await UnitOfWork.Courses.GetByTeacherIdAsync(teacherId, cancellationToken);
            Logger.LogInformation("Successfully retrieved {Count} courses for teacher: {TeacherId}", 
                courses.Count(), teacherId);

            return Mapper.Map<IEnumerable<CourseDto>>(courses);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error retrieving courses for teacher with ID: {TeacherId}", teacherId);
            throw;
        }
    }

    public async Task<IEnumerable<CourseDto>> GetCoursesByStudentAsync(Guid studentId,
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

            return Mapper.Map<IEnumerable<CourseDto>>(courses);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error retrieving courses for student with ID: {StudentId}", studentId);
            throw;
        }
    }
}