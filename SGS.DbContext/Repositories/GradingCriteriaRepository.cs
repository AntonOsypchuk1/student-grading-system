using Microsoft.EntityFrameworkCore;
using SGS.DbContext.Data;
using SGS.DbContext.Repositories.BaseRepository;
using SGS.DbContext.Repositories.Interfaces;
using SGS.Domain.Entities;

namespace SGS.DbContext.Repositories;

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
    
    public async Task AddCriteriaAsync(GradingCriteria criteria, CancellationToken cancellationToken = default)
    {
        if (criteria.AssignmentId == Guid.Empty)
            throw new ArgumentException("Criteria must be associated with an assignment");
            
        if (string.IsNullOrEmpty(criteria.Description))
            throw new ArgumentException("Criteria must have a description");
            
        await CreateAsync(criteria, cancellationToken);
    }

    public async Task AddCriteriaBatchAsync(IEnumerable<GradingCriteria> criteriaList,
        CancellationToken cancellationToken = default)
    {
        if (criteriaList == null || !criteriaList.Any())
            throw new ArgumentException("Criteria list cannot be null or empty");
        
        foreach (var criteria in criteriaList)
        {
            if (criteria.AssignmentId == Guid.Empty)
                throw new ArgumentException($"Criteria with description '{criteria.Description}' must be associated with an assignment");

            if (string.IsNullOrEmpty(criteria.Description))
                throw new ArgumentException($"Criteria for assignment {criteria.AssignmentId} must have a description");
        }

        await Context.GradingCriteria.AddRangeAsync(criteriaList, cancellationToken);
    }

    public Task UpdateCriteriaBatchAsync(IEnumerable<GradingCriteria> criteriaList, CancellationToken cancellationToken = default)
    {
        if (criteriaList == null || !criteriaList.Any())
            throw new ArgumentException("Criteria list cannot be null or empty");

        foreach (var criteria in criteriaList)
        {
            if (string.IsNullOrEmpty(criteria.Description))
                throw new ArgumentException($"Criteria for assignment {criteria.AssignmentId} must have a description");
        }

        Context.GradingCriteria.UpdateRange(criteriaList);
        return Context.SaveChangesAsync(cancellationToken);
    }
}