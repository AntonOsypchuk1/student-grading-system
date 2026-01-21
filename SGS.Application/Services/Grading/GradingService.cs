using AutoMapper;
using Microsoft.Extensions.Logging;
using SGS.Application.DTOs;
using SGS.Application.Interfaces;
using SGS.Application.Models;
using SGS.Domain.Entities;
using SGS.DbContext.Repositories.UnitOfWork;

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
        return await ExecuteInTransactionAsync(async () =>
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
                await UnitOfWork.SaveChangesAsync(cancellationToken);
                Logger.LogInformation("Updated grade ID: {GradeId} with edited grade: {EditedGrade}",
                    grade.Id, grade.EditedGrade);

                var analysisResults =
                    await UnitOfWork.AnalysisResults.GetByDocumentIdAsync(grade.DocumentId, cancellationToken);
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
        }, cancellationToken);
    }

    public async Task<Result<IEnumerable<GradingResultDto>>> GetGradingResultsByAssignmentAsync(Guid assignmentId,
        CancellationToken cancellationToken = default)
    {
        Logger.LogInformation("Retrieving grading results for assignment ID: {AssignmentId}", assignmentId);

        try
        {
            var assignment = await UnitOfWork.Assignments.GetByIdAsync(assignmentId, cancellationToken);
            if (assignment == null)
            {
                Logger.LogWarning("Assignment not found with ID: {AssignmentId}", assignmentId);
                throw new Exception("Assignment not found.");
            }

            var studentsInGroup = await UnitOfWork.StudentsGroups.GetByIdAsync(assignment.GroupId, cancellationToken);
            var gradingResults = new List<GradingResultDto>();
            if (studentsInGroup == null)
            {
                Logger.LogWarning("No students found in group for assignment ID: {AssignmentId}", assignmentId);
                return Result<IEnumerable<GradingResultDto>>.Success(gradingResults);
            }

            foreach (var student in studentsInGroup.Students)
            {
                var document = await UnitOfWork.Documents
                    .GetDocumentByUserIdAndAssignmentIdAsync(student.Id, assignmentId, cancellationToken);

                Grade? grade = null;
                if (document != null)
                {
                    var grades = await UnitOfWork.Grades.GetByDocumentIdAsync(document.Id, cancellationToken);
                    grade = grades.FirstOrDefault();
                }

                var gradingResult = new GradingResultDto
                {
                    Id = grade?.Id ?? Guid.NewGuid(),
                    AssignmentId = assignmentId,
                    StudentId = student.Id,
                    DocumentId = document?.Id ?? Guid.Empty,
                    StudentName = student.User.FullName,
                    GeneratedGrade = grade?.GeneratedGrade,
                    EditedGrade = grade?.EditedGrade,
                    IsFinalized = grade?.IsFinalized,
                    SubmissionDate = document?.UploadedAt
                };

                gradingResults.Add(gradingResult);
            }

            Logger.LogInformation("Retrieved grading results for {Count} students in assignment ID: {AssignmentId}", 
                gradingResults.Count, assignmentId);
            return Result<IEnumerable<GradingResultDto>>.Success(gradingResults);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error retrieving grading results for assignment ID: {AssignmentId}", assignmentId);
            return Result<IEnumerable<GradingResultDto>>.Failure($"Failed to retrieve grading results: {ex.Message}");
        }
    }

    public async Task<Result<CourseGradingDto>> GetGradingResultsByCourseAndGroupAsync(
        Guid courseId, Guid groupId, CancellationToken cancellationToken = default)
    {
        Logger.LogInformation("Retrieving grading results for course ID: {CourseId} and group ID: {GroupId}",
            courseId, groupId);

        try
        {
            var course = await UnitOfWork.Courses.GetByIdAsync(courseId, cancellationToken);
            if (course == null)
            {
                Logger.LogWarning("Course not found with ID: {CourseId}", courseId);
                throw new Exception("Course not found.");
            }

            var group = await UnitOfWork.StudentsGroups.GetByIdAsync(groupId, cancellationToken);
            if (group == null)
            {
                Logger.LogWarning("Group not found with ID: {GroupId}", groupId);
                throw new Exception("Group not found.");
            }

            var assignments =
                await UnitOfWork.Assignments.GetByGroupIdAndCourseIdAsync(groupId, courseId, cancellationToken);

            var courseGradingDto = new CourseGradingDto
            {
                CourseId = courseId,
                CourseName = course.Title,
                GroupId = groupId,
                GroupName = group.Name,
                AssignmentResults = new List<AssignmentResultsDto>()
            };

            foreach (var assignment in assignments)
            {
                var gradingResultsResult = await GetGradingResultsByAssignmentAsync(assignment.Id, cancellationToken);

                if (!gradingResultsResult.IsSuccess)
                {
                    Logger.LogWarning("Failed to retrieve grading results for assignment ID: {AssignmentId}",
                        assignment.Id);
                    continue;
                }

                var assignmentResultsDto = new AssignmentResultsDto
                {
                    Id = assignment.Id,
                    Results = gradingResultsResult.Data.ToList()
                };

                courseGradingDto.AssignmentResults.Add(assignmentResultsDto);
            }

            Logger.LogInformation(
                "Successfully retrieved course grading data for course ID: {CourseId} and group ID: {GroupId}",
                courseId, groupId);

            return Result<CourseGradingDto>.Success(courseGradingDto);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error retrieving grading results for course ID: {CourseId} and group ID: {GroupId}",
                courseId, groupId);
            return Result<CourseGradingDto>.Failure(
                $"Failed to retrieve course grading results: {ex.Message}");
        }
    }

    public async Task<Result<AnalysisResultDto?>> FinalizeGradeAsync(Guid gradeId,
        CancellationToken cancellationToken = default)
    {
        return await ExecuteInTransactionAsync(async () =>
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
        }, cancellationToken);
    }
}