using AutoMapper;
using Microsoft.Extensions.Logging;
using SGS.Application.DTOs;
using SGS.Application.Interfaces;
using SGS.Application.Models;
using SGS.Domain.Entities;
using SGS.Infrastructure.Repositories.UnitOfWork;

namespace SGS.Application.Services.Grading;

public class GradingService(
    IUnitOfWork unitOfWork,
    ILogger<GradingService> logger,
    IMapper mapper)
    : BaseService(unitOfWork, logger, mapper), IGradingService
{
    public async Task<Result<AnalysisResultDto?>> EditGradeAsync(EditGradeCommand command,
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

            return Result<AnalysisResultDto?>.Success(Mapper.Map<AnalysisResultDto>(analysisResult));
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error editing grade with ID: {GradeId}", command.GradeId);
            return Result<AnalysisResultDto?>.Failure($"Failed to edit grade: {ex.Message}");
        }
    }

    public async Task<Result<IEnumerable<AnalysisResultDto>>> GetGradingResultsByAssignmentAsync(
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
            return Result<IEnumerable<AnalysisResultDto>>.Success(Mapper.Map<IEnumerable<AnalysisResultDto>>(allResults));
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error retrieving grading results for assignment ID: {AssignmentId}", assignmentId);
            return Result<IEnumerable<AnalysisResultDto>>.Failure($"Failed to retrieve grading results: {ex.Message}");
        }
    }

    public async Task<Result<AnalysisResultDto?>> FinalizeGradeAsync(Guid gradeId,
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

            return Result<AnalysisResultDto?>.Success(Mapper.Map<AnalysisResultDto>(analysisResults.First()));
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error finalizing grade with ID: {GradeId}", gradeId);
            return Result<AnalysisResultDto?>.Failure($"Failed to finalize grade: {ex.Message}");
        }
    }
}