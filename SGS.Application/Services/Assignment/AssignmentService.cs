using AutoMapper;
using Microsoft.Extensions.Logging;
using SGS.Application.DTOs;
using SGS.Application.Interfaces;
using SGS.Application.Models;
using SGS.Domain.Entities;
using SGS.Infrastructure.Repositories.UnitOfWork;

namespace SGS.Application.Services.Assignment;

public class AssignmentService(
    IUnitOfWork unitOfWork,
    ILogger<AssignmentService> logger,
    IMapper mapper)
    : BaseService(unitOfWork, logger, mapper), IAssignmentService
{
    public async Task<Result<AssignmentDto>> CreateAssignmentAsync(CreateAssignmentCommand command,
        CancellationToken cancellationToken = default)
    {
        return await ExecuteInTransactionAsync(async () =>
        {
            Logger.LogInformation("Creating new assignment with title: {Title}", command.Title);

            try
            {
                var assignment = new Domain.Entities.Assignment
                {
                    Title = command.Title,
                    Description = command.Description,
                    MaxAttempts = command.MaxAttempts,
                    DueDate = DateTime.SpecifyKind(command.DueDate, DateTimeKind.Utc),
                    GroupId = command.GroupId,
                    CourseId = command.CourseId
                };

                await UnitOfWork.Assignments.AddAssignmentAsync(assignment, cancellationToken);
                Logger.LogInformation("Successfully created assignment with ID: {AssignmentId}", assignment.Id);
        
                return Result<AssignmentDto>.Success(Mapper.Map<AssignmentDto>(assignment));
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error creating assignment with title: {Title}", command.Title);
                return Result<AssignmentDto>.Failure($"Failed to create assignment: {ex.Message}");
            }
        }, cancellationToken);
    }

    public async Task<Result<AssignmentDto>> UpdateAssignmentAsync(UpdateAssignmentCommand command, CancellationToken cancellationToken = default)
    {
        return await ExecuteInTransactionAsync(async () =>
        {
            Logger.LogInformation("Updating assignment with ID: {AssignmentId}", command.Id);

            try
            {
                var assignment = await UnitOfWork.Assignments.GetByIdAsync(command.Id, cancellationToken);
                if (assignment == null)
                {
                    Logger.LogWarning("Assignment not found with ID: {AssignmentId}", command.Id);
                    return Result<AssignmentDto>.Failure($"Assignment with ID {command.Id} not found.");
                }

                assignment.Title = command.Title;
                assignment.Description = command.Description;
                assignment.MaxAttempts = command.MaxAttempts;
                assignment.DueDate = DateTime.SpecifyKind(command.DueDate, DateTimeKind.Utc);
                assignment.GroupId = command.GroupId;
                assignment.GradingCriteria = command.GradingCriteria.Select(Mapper.Map<GradingCriteria>).ToList();

                UnitOfWork.Assignments.Update(assignment);
                Logger.LogInformation("Successfully updated assignment with ID: {AssignmentId}", command.Id);

                return Result<AssignmentDto>.Success(Mapper.Map<AssignmentDto>(assignment));
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error updating assignment with ID: {AssignmentId}", command.Id);
                return Result<AssignmentDto>.Failure($"Failed to update assignment: {ex.Message}");
            }
        }, cancellationToken);
    }

    public async Task<Result<AssignmentDto?>> GetAssignmentByIdAsync(Guid assignmentId,
        CancellationToken cancellationToken = default)
    {
        Logger.LogInformation("Retrieving assignment with ID: {AssignmentId}", assignmentId);

        try
        {
            var assignment = await UnitOfWork.Assignments.GetByIdAsync(assignmentId, cancellationToken);
            if (assignment == null)
            {
                Logger.LogWarning("Assignment not found with ID: {AssignmentId}", assignmentId);
                return Result<AssignmentDto?>.Success(null);
            }
            
            Logger.LogInformation("Successfully retrieved assignment with ID: {AssignmentId}", assignmentId);
            return Result<AssignmentDto?>.Success(Mapper.Map<AssignmentDto>(assignment));
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error retrieving assignment with ID: {AssignmentId}", assignmentId);
            return Result<AssignmentDto?>.Failure($"Failed to retrieve assignment: {ex.Message}");
        }
    }

    public async Task<Result<IEnumerable<AssignmentDto>>> GetAssignmentsByGroupAndCourseAsync(Guid groupId, Guid courseId,
        CancellationToken cancellationToken = default)
    {
        return await ExecuteInTransactionAsync(async () =>
        {
            Logger.LogInformation("Retrieving assignments for group with ID: {GroupId}", groupId);

            try
            {
                var assignments = await UnitOfWork.Assignments
                    .GetByGroupIdAndCourseIdAsync(groupId, courseId, cancellationToken);
                Logger.LogInformation("Successfully retrieved {Count} assignments for group: {GroupId}",
                    assignments.Count(), groupId);

                return Result<IEnumerable<AssignmentDto>>.Success(Mapper.Map<IEnumerable<AssignmentDto>>(assignments));
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error retrieving assignments for group with ID: {GroupId}", groupId);
                return Result<IEnumerable<AssignmentDto>>.Failure($"Failed to retrieve assignments: {ex.Message}");
            }
        }, cancellationToken);
    }

    public Task<Result<IEnumerable<AssignmentDto>>> GetAssignmentsByCourseAsync(Guid courseId, CancellationToken cancellationToken = default)
    {
    Logger.LogInformation("Retrieving assignments for course with ID: {CourseId}", courseId);

        return ExecuteInTransactionAsync(async () =>
        {
            try
            {
                var assignments = await UnitOfWork.Assignments.GetByCourseIdAsync(courseId, cancellationToken);
                Logger.LogInformation("Successfully retrieved {Count} assignments for course: {CourseId}", 
                    assignments.Count(), courseId);

                return Result<IEnumerable<AssignmentDto>>.Success(Mapper.Map<IEnumerable<AssignmentDto>>(assignments));
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error retrieving assignments for course with ID: {CourseId}", courseId);
                return Result<IEnumerable<AssignmentDto>>.Failure($"Failed to retrieve assignments: {ex.Message}");
            }
        }, cancellationToken);
    }

    public Task<Result<IEnumerable<AssignmentDto>>> GetAssignmentsByGroupAsync(Guid groupId, CancellationToken cancellationToken = default)
    {
        Logger.LogInformation("Retrieving assignments for group with ID: {GroupId}", groupId);

        return ExecuteInTransactionAsync(async () =>
        {
            try
            {
                var assignments = await UnitOfWork.Assignments.GetByGroupIdAsync(groupId, cancellationToken);
                Logger.LogInformation("Successfully retrieved {Count} assignments for group: {GroupId}", 
                    assignments.Count(), groupId);

                return Result<IEnumerable<AssignmentDto>>.Success(Mapper.Map<IEnumerable<AssignmentDto>>(assignments));
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error retrieving assignments for group with ID: {GroupId}", groupId);
                return Result<IEnumerable<AssignmentDto>>.Failure($"Failed to retrieve assignments: {ex.Message}");
            }
        }, cancellationToken);
    }
}