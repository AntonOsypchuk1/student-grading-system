using SGS.Application.DTOs;
using SGS.Application.Models;

namespace SGS.Application.Interfaces;

public interface IAssignmentService
{
    Task<Result<AssignmentDto>> CreateAssignmentAsync(CreateAssignmentCommand command,
        CancellationToken cancellationToken = default);

    Task<Result<AssignmentDto?>> GetAssignmentByIdAsync(Guid assignmentId, CancellationToken cancellationToken = default);

    Task<Result<IEnumerable<AssignmentDto>>> GetAssignmentsByGroupAndCourseAsync(Guid groupId, Guid courseId,
        CancellationToken cancellationToken = default);
    
    Task<Result<IEnumerable<AssignmentDto>>> GetAssignmentsByCourseAsync(Guid courseId,
        CancellationToken cancellationToken = default);
    
    Task<Result<IEnumerable<AssignmentDto>>> GetAssignmentsByGroupAsync(Guid groupId,
        CancellationToken cancellationToken = default);
}