using SGS.Application.DTOs;

namespace SGS.Application.Interfaces;

public interface IGradingService
{
    Task<AnalysisResultDto> AnalyzeDocumentAsync(Guid documentId, CancellationToken cancellationToken = default);
    Task<AnalysisResultDto?> GetAnalysisResultByDocumentIdAsync(Guid documentId, CancellationToken cancellationToken = default);
    Task<AnalysisResultDto> EditGradeAsync(EditGradeCommand command, CancellationToken cancellationToken = default);
}