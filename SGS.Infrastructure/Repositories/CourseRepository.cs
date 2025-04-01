using Microsoft.EntityFrameworkCore;
using SGS.Domain.Entities;
using SGS.Infrastructure.Data;
using SGS.Infrastructure.Repositories.BaseRepository;
using SGS.Infrastructure.Repositories.Interfaces;

namespace SGS.Infrastructure.Repositories;

public class CourseRepository(AppDbContext context)
    : BaseRepository<Course>(context), ICourseRepository
{
    public async Task<IEnumerable<Course>> GetByTeacherIdAsync(Guid teacherId, CancellationToken cancellationToken = default)
    {
        return await Context.Courses
            .Where(c => c.Teachers.Any(t => t.Id == teacherId))
            .ToListAsync(cancellationToken);
    }
}