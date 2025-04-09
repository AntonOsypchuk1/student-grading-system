using SGS.Application.DTOs;

namespace SGS.Application.Interfaces;

public interface IGradingCriteriaService
{
    Task<GradingCriteriaDto> CreateCriteriaAsync(CreateGradingCriteriaCommand command, CancellationToken cancellationToken = default);
    
    Task<GradingCriteriaDto?> GetCriteriaByIdAsync(Guid id, CancellationToken cancellationToken = default);
    
    Task<IEnumerable<GradingCriteriaDto>> GetAllCriteriaAsync(CancellationToken cancellationToken = default);
}