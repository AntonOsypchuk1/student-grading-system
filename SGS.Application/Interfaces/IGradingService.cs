using SGS.Application.DTOs;
using SGS.Application.Models;

namespace SGS.Application.Interfaces;

public interface IGradingService
{
    Task<Result<AnalysisResultDto?>> EditGradeAsync(EditGradeCommand command, CancellationToken cancellationToken = default);

    Task<Result<IEnumerable<AnalysisResultDto>>> GetGradingResultsByAssignmentAsync(Guid assignmentId, CancellationToken cancellationToken = default);

    Task<Result<AnalysisResultDto?>> FinalizeGradeAsync(Guid gradeId, CancellationToken cancellationToken = default);
}