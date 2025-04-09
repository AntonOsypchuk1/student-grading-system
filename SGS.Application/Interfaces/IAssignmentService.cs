using SGS.Application.DTOs;

namespace SGS.Application.Interfaces;

public interface IAssignmentService
{
    Task<AssignmentDto> CreateAssignmentAsync(CreateAssignmentCommand command,
        CancellationToken cancellationToken = default);

    Task<AssignmentDto?> GetAssignmentByIdAsync(Guid assignmentId, CancellationToken cancellationToken = default);

    Task<IEnumerable<AssignmentDto>?> GetAssignmentsByGroupAsync(Guid groupId,
        CancellationToken cancellationToken = default);
}