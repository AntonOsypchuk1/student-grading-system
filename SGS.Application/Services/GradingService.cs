using AutoMapper;
using SGS.Application.DTOs;
using SGS.Application.Interfaces;
using SGS.Domain.Entities;
using SGS.Infrastructure.Repositories.Interfaces;

namespace SGS.Application.Services;

public class GradingService(
    IAnalysisResultsRepository analysisRepository,
    IGradeRepository gradeRepository,
    IDocumentRepository documentRepository,
    IMapper mapper,
    IAIService aiService)
    : IGradingService
{
    public async Task<AnalysisResultDto> AnalyzeDocumentAsync(Guid documentId,
        CancellationToken cancellationToken = default)
    {
        var document = await documentRepository.GetByIdAsync(documentId, cancellationToken);
        if (document == null)
            throw new Exception("Document not found.");

        var (summary, generatedGrade) = await aiService.AnalyzeDocumentAsync(document.FilePath, cancellationToken);

        var gradeEntity = new Grade
        {
            Id = Guid.NewGuid(),
            DocumentId = documentId,
            GeneratedGrade = generatedGrade,
            EditedGrade = null,
            IsFinalized = false
        };
        await gradeRepository.CreateAsync(gradeEntity, cancellationToken);

        // TODO: Select the appropriate criteria from assignment or course.
        var analysisEntity = new AnalysisResults
        {
            Id = Guid.NewGuid(),
            DocumentId = documentId,
            CriteriaId = Guid.NewGuid(), // For now, we use a dummy criteria ID.
            GradeId = gradeEntity.Id,
            GeneratedSummary = summary,
            IsApproved = false,
            CreatedAt = DateTime.UtcNow
        };
        await analysisRepository.CreateAsync(analysisEntity, cancellationToken);

        return mapper.Map<AnalysisResultDto>(analysisEntity);
    }

    public async Task<AnalysisResultDto?> GetAnalysisResultByDocumentIdAsync(Guid documentId,
        CancellationToken cancellationToken = default)
    {
        var results = await analysisRepository.GetByDocumentIdAsync(documentId, cancellationToken);
        var analysisResult = results?.FirstOrDefault();
        return analysisResult == null ? null : mapper.Map<AnalysisResultDto>(analysisResult);
    }

    public async Task<AnalysisResultDto> EditGradeAsync(EditGradeCommand command,
        CancellationToken cancellationToken = default)
    {
        var grade = await gradeRepository.GetByIdAsync(command.GradeId, cancellationToken);
        if (grade == null)
            throw new Exception("Grade not found.");

        grade.EditedGrade = command.EditedGrade;
        grade.IsFinalized = command.IsFinalized;
        await gradeRepository.UpdateAsync(grade, cancellationToken);

        var analysisResults = await analysisRepository.GetByDocumentIdAsync(grade.DocumentId, cancellationToken);
        var analysisResult = analysisResults?.FirstOrDefault();
        if (analysisResult == null)
            throw new Exception("Analysis result not found.");

        return mapper.Map<AnalysisResultDto>(analysisResult);
    }

    public async Task<IEnumerable<AnalysisResultDto>> GetGradingResultsByAssignmentAsync(Guid assignmentId, CancellationToken cancellationToken = default)
    {
        var documents = await documentRepository.GetDocumentsByAssignmentIdAsync(assignmentId, cancellationToken);
        var allResults = new List<AnalysisResults>();

        foreach (var doc in documents)
        {
            var results = (List<AnalysisResults>?)await analysisRepository.GetByDocumentIdAsync(doc.Id, cancellationToken) ;
            if (results is not null && results.Count != 0)
            {
                allResults.AddRange(results);
            }
        }
        return mapper.Map<IEnumerable<AnalysisResultDto>>(allResults);
    }

    public async Task<AnalysisResultDto> FinalizeGradeAsync(Guid gradeId, CancellationToken cancellationToken = default)
    {
        var grade = await gradeRepository.GetByIdAsync(gradeId, cancellationToken);
        if (grade is null)
            throw new Exception("Grade not found.");

        grade.IsFinalized = true;
        await gradeRepository.UpdateAsync(grade, cancellationToken);

        var analysisResults = (List<AnalysisResults>?)await analysisRepository.GetByDocumentIdAsync(grade.DocumentId, cancellationToken);
        if (analysisResults is null || analysisResults.Count == 0)
            throw new Exception("Associated analysis result not found.");

        var finalizedAnalysis = analysisResults.First();
        return mapper.Map<AnalysisResultDto>(finalizedAnalysis);
    }
}