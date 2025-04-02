using SGS.Domain.Entities;
using SGS.Infrastructure.Repositories.BaseRepository;

namespace SGS.Infrastructure.Repositories.Interfaces;

public interface ICourseRepository : IBaseRepository<Course>
{
    Task<IEnumerable<Course>> GetByTeacherIdAsync(Guid teacherId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Course>> GetByStudentIdAsync(Guid studentId, CancellationToken cancellationToken = default);
}