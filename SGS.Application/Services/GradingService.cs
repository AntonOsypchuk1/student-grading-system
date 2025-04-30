using AutoMapper;
using Microsoft.Extensions.Logging;
using SGS.Application.DTOs;
using SGS.Application.Interfaces;
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
    : IGradingService
{
    private readonly IDocumentRepository _documentRepository = unitOfWork.Documents;
    private readonly IAnalysisResultsRepository _analysisRepository = unitOfWork.AnalysisResults;
    private readonly IGradeRepository _gradeRepository = unitOfWork.Grades;
    
    public async Task<AnalysisResultDto?> AnalyzeDocumentAsync(Guid documentId,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Starting document analysis for document ID: {DocumentId}", documentId);

        try
        {
            var document = await _documentRepository.GetByIdAsync(documentId, cancellationToken);
            if (document == null)
            {
                logger.LogWarning("Document not found with ID: {DocumentId}", documentId);
                throw new Exception("Document not found.");
            }

            logger.LogInformation("Running AI analysis for document: {DocumentId}", documentId);
            var (summary, generatedGrade) = await aiService.AnalyzeDocumentAsync(document.FilePath, cancellationToken);

            var gradeEntity = new Grade
            {
                Id = Guid.NewGuid(),
                DocumentId = documentId,
                GeneratedGrade = generatedGrade,
                EditedGrade = null,
                IsFinalized = false
            };
            await _gradeRepository.CreateAsync(gradeEntity, cancellationToken);
            logger.LogInformation("Created grade record with ID: {GradeId} for document: {DocumentId}",
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
            await _analysisRepository.CreateAsync(analysisEntity, cancellationToken);
            logger.LogInformation("Created analysis result with ID: {AnalysisId} for document: {DocumentId}",
                analysisEntity.Id, documentId);

            return mapper.Map<AnalysisResultDto>(analysisEntity);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error analyzing document with ID: {DocumentId}", documentId);
            throw;
        }
    }

    public async Task<AnalysisResultDto?> GetAnalysisResultByDocumentIdAsync(Guid documentId,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Retrieving analysis result for document ID: {DocumentId}", documentId);

        try
        {
            var results = await _analysisRepository.GetByDocumentIdAsync(documentId, cancellationToken);
            var analysisResult = results?.FirstOrDefault();

            if (analysisResult == null)
            {
                logger.LogWarning("No analysis result found for document ID: {DocumentId}", documentId);
                return null;
            }

            logger.LogInformation("Successfully retrieved analysis result for document ID: {DocumentId}", documentId);
            return mapper.Map<AnalysisResultDto>(analysisResult);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving analysis result for document ID: {DocumentId}", documentId);
            throw;
        }
    }

    public async Task<AnalysisResultDto?> EditGradeAsync(EditGradeCommand command,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Editing grade with ID: {GradeId}", command.GradeId);

        try
        {
            var grade = await _gradeRepository.GetByIdAsync(command.GradeId, cancellationToken);
            if (grade == null)
            {
                logger.LogWarning("Grade not found with ID: {GradeId}", command.GradeId);
                throw new Exception("Grade not found.");
            }

            grade.EditedGrade = command.EditedGrade;
            grade.IsFinalized = command.IsFinalized;
            _gradeRepository.Update(grade);
            logger.LogInformation("Updated grade ID: {GradeId} with edited grade: {EditedGrade}", 
                grade.Id, grade.EditedGrade);

            var analysisResults = await _analysisRepository.GetByDocumentIdAsync(grade.DocumentId, cancellationToken);
            var analysisResult = analysisResults?.FirstOrDefault();
            if (analysisResult == null)
            {
                logger.LogWarning("Analysis result not found for document ID: {DocumentId}", grade.DocumentId);
                throw new Exception("Analysis result not found.");
            }

            return mapper.Map<AnalysisResultDto>(analysisResult);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error editing grade with ID: {GradeId}", command.GradeId);
            throw;
        }
    }

    public async Task<IEnumerable<AnalysisResultDto>> GetGradingResultsByAssignmentAsync(
        Guid assignmentId, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Retrieving grading results for assignment ID: {AssignmentId}", assignmentId);

        try
        {
            var documents = await _documentRepository.GetDocumentsByAssignmentIdAsync(assignmentId, cancellationToken);
            var allResults = new List<AnalysisResults>();

            foreach (var doc in documents)
            {
                var results = (List<AnalysisResults>?)await _analysisRepository.GetByDocumentIdAsync(doc.Id, cancellationToken);
                if (results is not null && results.Count != 0)
                {
                    allResults.AddRange(results);
                }
            }

            logger.LogInformation("Retrieved {Count} grading results for assignment ID: {AssignmentId}", 
                allResults.Count, assignmentId);
            return mapper.Map<IEnumerable<AnalysisResultDto>>(allResults);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving grading results for assignment ID: {AssignmentId}", assignmentId);
            throw;
        }
    }

    public async Task<AnalysisResultDto?> FinalizeGradeAsync(Guid gradeId,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Finalizing grade with ID: {GradeId}", gradeId);

        try
        {
            var grade = await _gradeRepository.GetByIdAsync(gradeId, cancellationToken);
            if (grade is null)
            {
                logger.LogWarning("Grade not found with ID: {GradeId}", gradeId);
                throw new Exception("Grade not found.");
            }

            grade.IsFinalized = true;
            _gradeRepository.Update(grade);
            logger.LogInformation("Grade finalized with ID: {GradeId}", gradeId);

            var analysisResults = (List<AnalysisResults>?)await _analysisRepository
                .GetByDocumentIdAsync(grade.DocumentId, cancellationToken);
            if (analysisResults is null || analysisResults.Count == 0)
            {
                logger.LogWarning("No analysis results found for document ID: {DocumentId}", grade.DocumentId);
                throw new Exception("Associated analysis result not found.");
            }

            return mapper.Map<AnalysisResultDto>(analysisResults.First());
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error finalizing grade with ID: {GradeId}", gradeId);
            throw;
        }
    }
}