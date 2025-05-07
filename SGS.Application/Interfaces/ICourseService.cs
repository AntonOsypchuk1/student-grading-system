using SGS.Application.DTOs;
using SGS.Application.Helpers;

namespace SGS.Application.Interfaces;

public interface ICourseService
{
    Task<Result<CourseDto>> CreateCourseAsync(CreateCourseCommand command, CancellationToken cancellationToken = default);
    Task<Result<CourseDto?>> GetCourseByIdAsync(Guid courseId, CancellationToken cancellationToken = default);

    Task<Result<IEnumerable<CourseDto>>>
        GetCoursesByTeacherAsync(Guid teacherId, CancellationToken cancellationToken = default);

    Task<Result<IEnumerable<CourseDto>>>
        GetCoursesByStudentAsync(Guid studentId, CancellationToken cancellationToken = default);
}