using AutoMapper;
using Microsoft.Extensions.Logging;
using SGS.Application.DTOs;
using SGS.Application.Interfaces;
using SGS.Domain.Entities;
using SGS.Infrastructure.Repositories.Interfaces;

namespace SGS.Application.Services;

public class CourseService(
    ICourseRepository courseRepository,
    IUserRepository userRepository,
    ILogger<CourseService> logger,
    IMapper mapper)
    : ICourseService
{
    // TODO: May be used to lookup teacher/student associations
    private readonly IUserRepository _userRepository = userRepository;

    public async Task<CourseDto> CreateCourseAsync(CreateCourseCommand command,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Creating course with title: {Title}", command.Title);
        try
        {
            var course = new Course
            {
                Id = Guid.NewGuid(),
                Title = command.Title,
                Description = command.Description
            };

            await courseRepository.CreateAsync(course, cancellationToken);
            logger.LogInformation("Successfully created course with ID: {CourseId}", course.Id);

            return mapper.Map<CourseDto>(course);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating course with title: {Title}", command.Title);
            throw;
        }
    }

    public async Task<CourseDto?> GetCourseByIdAsync(Guid courseId, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Retrieving course with ID: {CourseId}", courseId);

        try
        {
            var course = await courseRepository.GetByIdAsync(courseId, cancellationToken);

            if (course == null)
            {
                logger.LogWarning("Course not found with ID: {CourseId}", courseId);
                return null;
            }

            logger.LogInformation("Successfully retrieved course with ID: {CourseId}", courseId);
            return mapper.Map<CourseDto>(course);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving course with ID: {CourseId}", courseId);
            throw;
        }
    }

    public async Task<IEnumerable<CourseDto>> GetCoursesByTeacherAsync(Guid teacherId,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Retrieving courses for teacher with ID: {TeacherId}", teacherId);

        try
        {
            var courses = await courseRepository.GetByTeacherIdAsync(teacherId, cancellationToken);
            logger.LogInformation("Successfully retrieved {Count} courses for teacher: {TeacherId}", 
                courses.Count(), teacherId);

            return mapper.Map<IEnumerable<CourseDto>>(courses);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving courses for teacher with ID: {TeacherId}", teacherId);
            throw;
        }
    }

    public async Task<IEnumerable<CourseDto>> GetCoursesByStudentAsync(Guid studentId,
        CancellationToken cancellationToken = default)
    {
        // For simplicity, we assume that the student is associated with one or more groups,
        // and groups are associated with courses via a many-to-many relationship.
        logger.LogInformation("Retrieving courses for student with ID: {StudentId}", studentId);

        try
        {
            var courses = await courseRepository.GetByStudentIdAsync(studentId, cancellationToken);
            logger.LogInformation("Successfully retrieved {Count} courses for student: {StudentId}", 
                courses.Count(), studentId);

            return mapper.Map<IEnumerable<CourseDto>>(courses);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving courses for student with ID: {StudentId}", studentId);
            throw;
        }
    }
}