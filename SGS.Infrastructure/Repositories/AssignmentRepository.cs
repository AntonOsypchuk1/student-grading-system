using Microsoft.EntityFrameworkCore;
using SGS.Domain.Entities;
using SGS.Infrastructure.Data;
using SGS.Infrastructure.Repositories.BaseRepository;
using SGS.Infrastructure.Repositories.Interfaces;

namespace SGS.Infrastructure.Repositories;

public class AssignmentRepository(AppDbContext context)
    : BaseRepository<Assignment>(context), IAssignmentRepository
{
    public async Task<IEnumerable<Assignment>> GetByGroupIdAsync(Guid groupId,
        CancellationToken cancellationToken = default)
    {
        var assignments = await Context.Assignments.ToListAsync(cancellationToken);
        return await Context.Assignments
            .Where(a => a.GroupId == groupId)
            .ToListAsync(cancellationToken);
    }
}