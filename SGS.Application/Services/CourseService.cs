using AutoMapper;
using SGS.Application.DTOs;
using SGS.Application.Interfaces;
using SGS.Domain.Entities;
using SGS.Infrastructure.Repositories.Interfaces;

namespace SGS.Application.Services;

public class CourseService(ICourseRepository courseRepository, IUserRepository userRepository, IMapper mapper)
    : ICourseService
{
    private readonly IUserRepository _userRepository = userRepository; // May be used to lookup teacher/student associations

    public async Task<CourseDto> CreateCourseAsync(CreateCourseCommand command, CancellationToken cancellationToken = default)
    {
        var course = new Course
        {
            Id = Guid.NewGuid(),
            Title = command.Title,
            Description = command.Description
        };

        await courseRepository.CreateAsync(course, cancellationToken);
        return mapper.Map<CourseDto>(course);
    }

    public async Task<CourseDto?> GetCourseByIdAsync(Guid courseId, CancellationToken cancellationToken = default)
    {
        var course = await courseRepository.GetByIdAsync(courseId, cancellationToken);
        return course == null ? null : mapper.Map<CourseDto>(course);
    }

    public async Task<IEnumerable<CourseDto>> GetCoursesByTeacherAsync(Guid teacherId, CancellationToken cancellationToken = default)
    {
        // Assuming that ICourseRepository has a method for this.
        var courses = await courseRepository.GetByTeacherIdAsync(teacherId, cancellationToken);
        return mapper.Map<IEnumerable<CourseDto>>(courses);
    }

    public async Task<IEnumerable<CourseDto>> GetCoursesByStudentAsync(Guid studentId, CancellationToken cancellationToken = default)
    {
        // For simplicity, we assume that the student is associated with one or more groups,
        // and groups are associated with courses via a many-to-many relationship.
        // You would implement this logic in your repository or service.
        var courses = await courseRepository.GetByStudentIdAsync(studentId, cancellationToken);
        return mapper.Map<IEnumerable<CourseDto>>(courses);
    }
}
