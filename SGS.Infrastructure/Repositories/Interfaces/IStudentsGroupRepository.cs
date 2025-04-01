using SGS.Domain.Entities;
using SGS.Infrastructure.Repositories.BaseRepository;

namespace SGS.Infrastructure.Repositories.Interfaces;

public interface IStudentsGroupRepository : IBaseRepository<StudentsGroup>
{
    Task<IEnumerable<StudentsGroup>> GetGroupsByTeacherIdAsync(Guid teacherId, CancellationToken cancellationToken = default);
}