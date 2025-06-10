using SGS.Domain.Entities;
using SGS.Infrastructure.Repositories.BaseRepository;

namespace SGS.Infrastructure.Repositories.Interfaces;

public interface IStudentsGroupRepository : IBaseRepository<StudentsGroup>
{
    Task<StudentsGroup?> GetByStudentIdAsync(Guid studentId,
        CancellationToken cancellationToken = default);

    Task<StudentsGroup?> GetByGroupByNameAsync(string name,
        CancellationToken cancellationToken = default);

    Task<IEnumerable<StudentsGroup>> GetByCourseIdAsync(Guid courseId,
        CancellationToken cancellationToken = default);

    Task AddStudentToGroupAsync(Guid groupId, Guid studentId,
        CancellationToken cancellationToken = default);

    Task AddGroupAsync(StudentsGroup group,
        CancellationToken cancellationToken = default);

    Task RemoveStudentFromGroupAsync(Guid studentId, Guid groupId,
        CancellationToken cancellationToken = default);
}