using SGS.Application.DTOs;
using SGS.Application.Models;

namespace SGS.Application.Interfaces;

public interface ICourseService
{
    Task<Result<CourseDto>> CreateCourseAsync(CreateCourseCommand command,
        CancellationToken cancellationToken = default);
    
    Task<Result<IEnumerable<CourseDto>>> GetAllCoursesAsync(
        CancellationToken cancellationToken = default);
    
    Task<Result<CourseDto?>> GetCourseByIdAsync(Guid courseId,
        CancellationToken cancellationToken = default);

    Task<Result<IEnumerable<CourseDto>>> GetCoursesByTeacherAsync(Guid teacherId,
        CancellationToken cancellationToken = default);

    Task<Result<IEnumerable<CourseDto>>> GetCoursesByStudentAsync(Guid studentId,
        CancellationToken cancellationToken = default);
    
    Task<Result<bool>> AssignTeacherToCourseAsync(Guid teacherId, Guid courseId,
        CancellationToken cancellationToken = default);
    // Task<Result<bool>> RemoveTeacherFromCourseAsync(Guid teacherId, Guid courseId,
    //     CancellationToken cancellationToken = default);
    
    Task<Result<bool>> AssignGroupToCourseAsync(Guid groupId, Guid courseId,
        CancellationToken cancellationToken = default);
    
    // Task<Result<bool>> RemoveGroupFromCourseAsync(Guid groupId, Guid courseId,
    //     CancellationToken cancellationToken = default);
}