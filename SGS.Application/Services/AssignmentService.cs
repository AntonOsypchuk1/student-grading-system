using AutoMapper;
using SGS.Application.DTOs;
using SGS.Application.Interfaces;
using SGS.Domain.Entities;
using SGS.Infrastructure.Repositories.Interfaces;

namespace SGS.Application.Services;

public class AssignmentService(IAssignmentRepository assignmentRepository, IMapper mapper) : IAssignmentService
{
    public async Task<AssignmentDto> CreateAssignmentAsync(CreateAssignmentCommand command,
        CancellationToken cancellationToken = default)
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

        await assignmentRepository.CreateAsync(assignment, cancellationToken);
        return mapper.Map<AssignmentDto>(assignment);
    }

    public async Task<AssignmentDto?> GetAssignmentByIdAsync(Guid assignmentId,
        CancellationToken cancellationToken = default)
    {
        var assignment = await assignmentRepository.GetByIdAsync(assignmentId, cancellationToken);
        return assignment == null ? null : mapper.Map<AssignmentDto>(assignment);
    }

    public async Task<IEnumerable<AssignmentDto>> GetAssignmentsByGroupAsync(Guid groupId,
        CancellationToken cancellationToken = default)
    {
        var assignments = await assignmentRepository.GetByGroupIdAsync(groupId, cancellationToken);
        return mapper.Map<IEnumerable<AssignmentDto>>(assignments);
    }
}