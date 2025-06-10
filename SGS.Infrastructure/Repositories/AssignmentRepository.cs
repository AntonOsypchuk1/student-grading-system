using Microsoft.EntityFrameworkCore;
using SGS.Domain.Entities;
using SGS.Infrastructure.Data;
using SGS.Infrastructure.Repositories.BaseRepository;
using SGS.Infrastructure.Repositories.Interfaces;

namespace SGS.Infrastructure.Repositories;

public class AssignmentRepository(AppDbContext context)
    : BaseRepository<Assignment>(context), IAssignmentRepository
{
    // public override async Task<IEnumerable<Assignment>> GetAllAsync(CancellationToken cancellationToken = default)
    // {
    //     return await Context.Assignments
    //         .Include(a => a.Documents)
    //         .Include(a => a.Group)
    //         .Include(a => a.Course)
    //         .ToListAsync(cancellationToken);
    // }
    
    public async Task<IEnumerable<Assignment>> GetByGroupIdAndCourseIdAsync(Guid groupId, Guid courseId,
        CancellationToken cancellationToken = default)
    {
        var assignments = await Context.Assignments
            .Where(a => a.GroupId == groupId && a.CourseId == courseId)
            .ToListAsync(cancellationToken);
        
        return assignments;
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