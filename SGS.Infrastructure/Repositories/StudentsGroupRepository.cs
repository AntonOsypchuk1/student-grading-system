using Microsoft.EntityFrameworkCore;
using SGS.Domain.Entities;
using SGS.Infrastructure.Data;
using SGS.Infrastructure.Repositories.BaseRepository;
using SGS.Infrastructure.Repositories.Interfaces;

namespace SGS.Infrastructure.Repositories;

public class StudentsGroupRepository(AppDbContext context)
    : BaseRepository<StudentsGroup>(context), IStudentsGroupRepository
{
    public async Task<IEnumerable<StudentsGroup>> GetGroupsByTeacherIdAsync(Guid teacherId,
        CancellationToken cancellationToken = default)
    {
        return await Context.StudentGroups
            .Where(g => g.Teachers.Any(t => t.Id == teacherId))
            .ToListAsync(cancellationToken);
    }
}