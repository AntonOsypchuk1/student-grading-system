using AutoMapper;
using Microsoft.Extensions.Logging;
using SGS.Application.DTOs;
using SGS.Application.Interfaces;
using SGS.Application.Models;
using SGS.Application.Services.Grading;
using SGS.Domain.Entities;
using SGS.Infrastructure.Repositories.UnitOfWork;

namespace SGS.Application.Services.Analysis;

public class AnalysisService(
    IAIService aiService,
    IUnitOfWork unitOfWork,
    ILogger<GradingService> logger,
    IMapper mapper)
    : BaseService(unitOfWork, logger, mapper), IAnalysisService
{
    public async Task<Result<AnalysisResultDto?>> AnalyzeDocumentAsync(Guid documentId,
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
                
                // Get the assignment related to this document
                var assignment = await UnitOfWork.Assignments.GetByIdAsync(document.AssignmentId, cancellationToken);
                if (assignment == null)
                {
                    Logger.LogWarning("Assignment not found for document ID: {DocumentId}", documentId);
                    throw new Exception("Assignment not found.");
                }

                // Get available criteria for this assignment
                if (await UnitOfWork.GradingCriteria.GetByAssignmentIdAsync(assignment.Id, cancellationToken)
                        is not List<GradingCriteria> criteria || 
                    criteria.Count == 0)
                {
                    Logger.LogWarning("No grading criteria found for assignment ID: {AssignmentId}", assignment.Id);
                    throw new Exception("No grading criteria found for this assignment.");
                }
                
                Logger.LogInformation("Running AI analysis for document: {DocumentId}", documentId);
                var (summary, generatedGrade) = await aiService.AnalyzeDocumentAsync(document, criteria, cancellationToken);

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
                    CriteriaId = criteria.First().Id, // TODO: Possibly remove it
                    GradeId = gradeEntity.Id,
                    GeneratedSummary = summary,
                    IsApproved = false,
                    CreatedAt = DateTime.UtcNow
                };
                await UnitOfWork.AnalysisResults.CreateAsync(analysisEntity, cancellationToken);
                Logger.LogInformation("Created analysis result with ID: {AnalysisId} for document: {DocumentId}",
                    analysisEntity.Id, documentId);

                return Result<AnalysisResultDto?>.Success(Mapper.Map<AnalysisResultDto>(analysisEntity));
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error analyzing document with ID: {DocumentId}", documentId);
                return Result<AnalysisResultDto?>.Failure($"Failed to analyze document: {ex.Message}");
            }
        }, cancellationToken);
    }

    public async Task<Result<AnalysisResultDto?>> GetAnalysisResultByDocumentIdAsync(Guid documentId,
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
                return Result<AnalysisResultDto?>.Success(null);
            }

            Logger.LogInformation("Successfully retrieved analysis result for document ID: {DocumentId}", documentId);
            return Result<AnalysisResultDto?>.Success(Mapper.Map<AnalysisResultDto>(analysisResult));
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error retrieving analysis result for document ID: {DocumentId}", documentId);
            return Result<AnalysisResultDto?>.Failure($"Failed to retrieve analysis result: {ex.Message}");
        }
    }
}