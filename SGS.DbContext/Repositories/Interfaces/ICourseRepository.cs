using SGS.DbContext.Repositories.BaseRepository;
using SGS.Domain.Entities;

namespace SGS.DbContext.Repositories.Interfaces;

public interface ICourseRepository : IBaseRepository<Course>
{
    Task<IEnumerable<Course>> GetByTeacherIdAsync(Guid teacherId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Course>> GetByStudentIdAsync(Guid studentId, CancellationToken cancellationToken = default);
    Task AddTeacherToCourseAsync(Guid courseId, Guid teacherId, CancellationToken cancellationToken = default);
    Task AddStudentsGroupToCourseAsync(Guid courseId, Guid groupId, CancellationToken cancellationToken = default);
    Task AddCourseAsync(Course course, CancellationToken cancellationToken = default);
}