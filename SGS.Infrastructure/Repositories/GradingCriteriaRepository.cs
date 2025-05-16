using Microsoft.EntityFrameworkCore;
using SGS.Domain.Entities;
using SGS.Infrastructure.Data;
using SGS.Infrastructure.Repositories.BaseRepository;
using SGS.Infrastructure.Repositories.Interfaces;

namespace SGS.Infrastructure.Repositories;

public class GradingCriteriaRepository(AppDbContext context)
    : BaseRepository<GradingCriteria>(context), IGradingCriteriaRepository
{
    public async Task<IEnumerable<GradingCriteria>> GetByAssignmentIdAsync(Guid assignmentId,
        CancellationToken cancellationToken = default)
    {
        return await Context.GradingCriteria
            .Where(gc => gc.AssignmentId == assignmentId)
            .ToListAsync(cancellationToken);
    }
}