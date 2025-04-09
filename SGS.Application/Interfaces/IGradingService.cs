using SGS.Application.DTOs;

namespace SGS.Application.Interfaces;

public interface IGradingService
{
    Task<AnalysisResultDto> AnalyzeDocumentAsync(Guid documentId, CancellationToken cancellationToken = default);

    Task<AnalysisResultDto?> GetAnalysisResultByDocumentIdAsync(Guid documentId,
        CancellationToken cancellationToken = default);

    Task<AnalysisResultDto> EditGradeAsync(EditGradeCommand command, CancellationToken cancellationToken = default);

    Task<IEnumerable<AnalysisResultDto>> GetGradingResultsByAssignmentAsync(Guid assignmentId, CancellationToken cancellationToken = default);

    Task<AnalysisResultDto> FinalizeGradeAsync(Guid gradeId, CancellationToken cancellationToken = default);
}