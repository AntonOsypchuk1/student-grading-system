using AutoMapper;
using Microsoft.Extensions.Logging;
using SGS.Application.DTOs;
using SGS.Application.Interfaces;
using SGS.Domain.Entities;
using SGS.Infrastructure.Data;
using SGS.Infrastructure.Repositories.Interfaces;
using SGS.Infrastructure.Repositories.UnitOfWork;

namespace SGS.Application.Services;

public class AssignmentService(
    IUnitOfWork unitOfWork,
    ILogger<AssignmentService> logger,
    IMapper mapper)
    : IAssignmentService
{
    private readonly IAssignmentRepository _assignmentRepository = unitOfWork.Assignments;
    
    public async Task<AssignmentDto> CreateAssignmentAsync(CreateAssignmentCommand command,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Creating new assignment with title: {Title}", command.Title);

        try
        {
            var assignment = new Assignment
            {
                Id = Guid.NewGuid(),
                Title = command.Title,
                Description = command.Description,
                MaxAttempts = command.MaxAttempts,
                DueDate = command.DueDate,
                GroupId = command.GroupId,
                CourseId = command.CourseId
            };

            await _assignmentRepository.CreateAsync(assignment, cancellationToken);
            logger.LogInformation("Successfully created assignment with ID: {AssignmentId}", assignment.Id);
        
            return mapper.Map<AssignmentDto>(assignment);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating assignment with title: {Title}", command.Title);
            throw;
        }
    }

    public async Task<AssignmentDto?> GetAssignmentByIdAsync(Guid assignmentId,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Retrieving assignment with ID: {AssignmentId}", assignmentId);

        try
        {
            var assignment = await _assignmentRepository.GetByIdAsync(assignmentId, cancellationToken);
            if (assignment == null)
            {
                logger.LogWarning("Assignment not found with ID: {AssignmentId}", assignmentId);
                return null;
            }
            logger.LogInformation("Successfully retrieved assignment with ID: {AssignmentId}", assignmentId);
            
            return mapper.Map<AssignmentDto>(assignment);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving assignment with ID: {AssignmentId}", assignmentId);
            throw;
        }
    }

    public async Task<IEnumerable<AssignmentDto>?> GetAssignmentsByGroupAsync(Guid groupId,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Retrieving assignments for group with ID: {GroupId}", groupId);

        try
        {
            var assignments = await _assignmentRepository.GetByGroupIdAsync(groupId, cancellationToken);
            logger.LogInformation("Successfully retrieved {Count} assignments for group: {GroupId}", 
                assignments.Count(), groupId);

            return mapper.Map<IEnumerable<AssignmentDto>>(assignments);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving assignments for group with ID: {GroupId}", groupId);
            throw;
        }
    }
}