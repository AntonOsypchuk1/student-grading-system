using Microsoft.EntityFrameworkCore;
using SGS.DbContext.Data;
using SGS.DbContext.Repositories.BaseRepository;
using SGS.DbContext.Repositories.Interfaces;
using SGS.Domain.Entities;

namespace SGS.DbContext.Repositories;

public class AssignmentRepository(AppDbContext context)
    : BaseRepository<Assignment>(context), IAssignmentRepository
{
    public override Task<Assignment?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return Context.Assignments
            .Include(a => a.GradingCriteria)
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<Assignment>> GetByGroupIdAndCourseIdAsync(Guid groupId, Guid courseId,
        CancellationToken cancellationToken = default)
    {
        return await Context.Assignments
            .Where(a => a.GroupId == groupId && a.CourseId == courseId)
            .ToListAsync(cancellationToken);
    }
    
    public async Task AddAssignmentAsync(Assignment assignment, CancellationToken cancellationToken = default)
    {
        if (assignment.Title == null || assignment.Description == null)
            throw new ArgumentException("Assignment must have a title and description");
            
        if (assignment.DueDate == default)
            throw new ArgumentException("Assignment must have a due date");
            
        await CreateAsync(assignment, cancellationToken);
    }
    
    public async Task<IEnumerable<Assignment>> GetByGroupIdAsync(Guid groupId, CancellationToken cancellationToken = default)
    {
        return await Context.Assignments
            .Where(a => a.GroupId == groupId)
            .ToListAsync(cancellationToken);
    }
    
    public async Task<IEnumerable<Assignment>> GetByCourseIdAsync(Guid courseId, CancellationToken cancellationToken = default)
    {
        return await Context.Assignments
            .Where(a => a.CourseId == courseId)
            .ToListAsync(cancellationToken);
    }
}