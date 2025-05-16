using SGS.Application.DTOs;
using SGS.Application.Models;

namespace SGS.Application.Interfaces;

public interface IAnalysisService
{
    Task<Result<AnalysisResultDto?>> AnalyzeDocumentAsync(Guid documentId, CancellationToken cancellationToken = default);

    Task<Result<AnalysisResultDto?>> GetAnalysisResultByDocumentIdAsync(Guid documentId,
        CancellationToken cancellationToken = default);
}