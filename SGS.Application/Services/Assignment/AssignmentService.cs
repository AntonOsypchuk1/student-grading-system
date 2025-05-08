using AutoMapper;
using Microsoft.Extensions.Logging;
using SGS.Application.DTOs;
using SGS.Application.Interfaces;
using SGS.Application.Models;
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
                    Id = Guid.NewGuid(),
                    Title = command.Title,
                    Description = command.Description,
                    MaxAttempts = command.MaxAttempts,
                    DueDate = command.DueDate,
                    GroupId = command.GroupId,
                    CourseId = command.CourseId
                };

                await UnitOfWork.Assignments.CreateAsync(assignment, cancellationToken);
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

    public async Task<Result<IEnumerable<AssignmentDto>?>> GetAssignmentsByGroupAsync(Guid groupId,
        CancellationToken cancellationToken = default)
    {
        Logger.LogInformation("Retrieving assignments for group with ID: {GroupId}", groupId);

        try
        {
            var assignments = await UnitOfWork.Assignments.GetByGroupIdAsync(groupId, cancellationToken);
            Logger.LogInformation("Successfully retrieved {Count} assignments for group: {GroupId}", 
                assignments.Count(), groupId);

            return Result<IEnumerable<AssignmentDto>?>.Success(Mapper.Map<IEnumerable<AssignmentDto>>(assignments));
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error retrieving assignments for group with ID: {GroupId}", groupId);
            return Result<IEnumerable<AssignmentDto>?>.Failure($"Failed to retrieve assignments: {ex.Message}");
        }
    }
}