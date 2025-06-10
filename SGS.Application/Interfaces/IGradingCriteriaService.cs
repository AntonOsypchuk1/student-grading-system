using SGS.Application.DTOs;
using SGS.Application.Models;

namespace SGS.Application.Interfaces;

public interface IGradingCriteriaService
{
    Task<Result<GradingCriteriaDto>> CreateCriteriaAsync(CreateGradingCriteriaCommand command,
        CancellationToken cancellationToken = default);

    Task<Result<IEnumerable<GradingCriteriaDto>>> CreateCriteriaBatchAsync(CreateGradingCriteriaBatchCommand command,
        CancellationToken cancellationToken = default);
    
    Task<Result<GradingCriteriaDto?>> GetCriteriaByIdAsync(Guid id,
        CancellationToken cancellationToken = default);
    
    Task<Result<IEnumerable<GradingCriteriaDto>>> GetAllCriteriaAsync(CancellationToken cancellationToken = default);
    
    Task<Result<IEnumerable<GradingCriteriaDto>>> GetCriteriaByAssignmentAsync(Guid assignmentId,
        CancellationToken cancellationToken = default);
}