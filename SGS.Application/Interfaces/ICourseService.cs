using SGS.Application.DTOs;

namespace SGS.Application.Interfaces;

public interface ICourseService
{
    Task<CourseDto> CreateCourseAsync(CreateCourseCommand command, CancellationToken cancellationToken = default);
    Task<CourseDto?> GetCourseByIdAsync(Guid courseId, CancellationToken cancellationToken = default);

    Task<IEnumerable<CourseDto>>
        GetCoursesByTeacherAsync(Guid teacherId, CancellationToken cancellationToken = default);

    Task<IEnumerable<CourseDto>>
        GetCoursesByStudentAsync(Guid studentId, CancellationToken cancellationToken = default);
}