using AutoMapper;
using Microsoft.Extensions.Logging;
using SGS.Application.DTOs;
using SGS.Application.Interfaces;
using SGS.Application.Services.Bases;
using SGS.Domain.Entities;
using SGS.Infrastructure.Data;
using SGS.Infrastructure.Repositories.Interfaces;
using SGS.Infrastructure.Repositories.UnitOfWork;

namespace SGS.Application.Services;

public class GradingService(
    IUnitOfWork unitOfWork,
    IAIService aiService,
    ILogger<GradingService> logger,
    IMapper mapper)
    : BaseService(unitOfWork, logger, mapper), IGradingService
{
    public async Task<AnalysisResultDto?> AnalyzeDocumentAsync(Guid documentId,
        CancellationToken cancellationToken = default)
    {
        return await ExecuteInTransactionAsync(async () =>
        {
            Logger.LogInformation("Starting document analysis for document ID: {DocumentId}", documentId);
    
            try
            {
                var document = await UnitOfWork.Documents.GetByIdAsync(documentId, cancellationToken);
                if (document == null)
                {
                    Logger.LogWarning("Document not found with ID: {DocumentId}", documentId);
                    throw new Exception("Document not found.");
                }

                Logger.LogInformation("Running AI analysis for document: {DocumentId}", documentId);
                var (summary, generatedGrade) = await aiService.AnalyzeDocumentAsync(document.FilePath, cancellationToken);

                var gradeEntity = new Grade
                {
                    Id = Guid.NewGuid(),
                    DocumentId = documentId,
                    GeneratedGrade = generatedGrade,
                    EditedGrade = null,
                    IsFinalized = false
                };
                await UnitOfWork.Grades.CreateAsync(gradeEntity, cancellationToken);
                Logger.LogInformation("Created grade record with ID: {GradeId} for document: {DocumentId}",
                    gradeEntity.Id, documentId);

                var analysisEntity = new AnalysisResults
                {
                    Id = Guid.NewGuid(),
                    DocumentId = documentId,
                    CriteriaId = Guid.NewGuid(),
                    GradeId = gradeEntity.Id,
                    GeneratedSummary = summary,
                    IsApproved = false,
                    CreatedAt = DateTime.UtcNow
                };
                await UnitOfWork.AnalysisResults.CreateAsync(analysisEntity, cancellationToken);
                Logger.LogInformation("Created analysis result with ID: {AnalysisId} for document: {DocumentId}",
                    analysisEntity.Id, documentId);

                return Mapper.Map<AnalysisResultDto>(analysisEntity);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error analyzing document with ID: {DocumentId}", documentId);
                throw;
            }
        }, cancellationToken);
    }

    public async Task<AnalysisResultDto?> GetAnalysisResultByDocumentIdAsync(Guid documentId,
        CancellationToken cancellationToken = default)
    {
        Logger.LogInformation("Retrieving analysis result for document ID: {DocumentId}", documentId);

        try
        {
            var results = await UnitOfWork.AnalysisResults.GetByDocumentIdAsync(documentId, cancellationToken);
            var analysisResult = results?.FirstOrDefault();

            if (analysisResult == null)
            {
                Logger.LogWarning("No analysis result found for document ID: {DocumentId}", documentId);
                return null;
            }

            Logger.LogInformation("Successfully retrieved analysis result for document ID: {DocumentId}", documentId);
            return Mapper.Map<AnalysisResultDto>(analysisResult);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error retrieving analysis result for document ID: {DocumentId}", documentId);
            throw;
        }
    }

    public async Task<AnalysisResultDto?> EditGradeAsync(EditGradeCommand command,
        CancellationToken cancellationToken = default)
    {
        Logger.LogInformation("Editing grade with ID: {GradeId}", command.GradeId);

        try
        {
            var grade = await UnitOfWork.Grades.GetByIdAsync(command.GradeId, cancellationToken);
            if (grade == null)
            {
                Logger.LogWarning("Grade not found with ID: {GradeId}", command.GradeId);
                throw new Exception("Grade not found.");
            }

            grade.EditedGrade = command.EditedGrade;
            grade.IsFinalized = command.IsFinalized;
            UnitOfWork.Grades.Update(grade);
            Logger.LogInformation("Updated grade ID: {GradeId} with edited grade: {EditedGrade}", 
                grade.Id, grade.EditedGrade);

            var analysisResults = await UnitOfWork.AnalysisResults.GetByDocumentIdAsync(grade.DocumentId, cancellationToken);
            var analysisResult = analysisResults?.FirstOrDefault();
            if (analysisResult == null)
            {
                Logger.LogWarning("Analysis result not found for document ID: {DocumentId}", grade.DocumentId);
                throw new Exception("Analysis result not found.");
            }

            return Mapper.Map<AnalysisResultDto>(analysisResult);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error editing grade with ID: {GradeId}", command.GradeId);
            throw;
        }
    }

    public async Task<IEnumerable<AnalysisResultDto>> GetGradingResultsByAssignmentAsync(
        Guid assignmentId, CancellationToken cancellationToken = default)
    {
        Logger.LogInformation("Retrieving grading results for assignment ID: {AssignmentId}", assignmentId);

        try
        {
            var documents = await UnitOfWork.Documents.GetDocumentsByAssignmentIdAsync(assignmentId, cancellationToken);
            var allResults = new List<AnalysisResults>();

            foreach (var doc in documents)
            {
                var results = (List<AnalysisResults>?)await UnitOfWork.AnalysisResults.GetByDocumentIdAsync(doc.Id, cancellationToken);
                if (results is not null && results.Count != 0)
                {
                    allResults.AddRange(results);
                }
            }

            Logger.LogInformation("Retrieved {Count} grading results for assignment ID: {AssignmentId}", 
                allResults.Count, assignmentId);
            return Mapper.Map<IEnumerable<AnalysisResultDto>>(allResults);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error retrieving grading results for assignment ID: {AssignmentId}", assignmentId);
            throw;
        }
    }

    public async Task<AnalysisResultDto?> FinalizeGradeAsync(Guid gradeId,
        CancellationToken cancellationToken = default)
    {
        Logger.LogInformation("Finalizing grade with ID: {GradeId}", gradeId);

        try
        {
            var grade = await UnitOfWork.Grades.GetByIdAsync(gradeId, cancellationToken);
            if (grade is null)
            {
                Logger.LogWarning("Grade not found with ID: {GradeId}", gradeId);
                throw new Exception("Grade not found.");
            }

            grade.IsFinalized = true;
            UnitOfWork.Grades.Update(grade);
            Logger.LogInformation("Grade finalized with ID: {GradeId}", gradeId);

            var analysisResults = (List<AnalysisResults>?)await UnitOfWork.AnalysisResults
                .GetByDocumentIdAsync(grade.DocumentId, cancellationToken);
            if (analysisResults is null || analysisResults.Count == 0)
            {
                Logger.LogWarning("No analysis results found for document ID: {DocumentId}", grade.DocumentId);
                throw new Exception("Associated analysis result not found.");
            }

            return Mapper.Map<AnalysisResultDto>(analysisResults.First());
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error finalizing grade with ID: {GradeId}", gradeId);
            throw;
        }
    }
}