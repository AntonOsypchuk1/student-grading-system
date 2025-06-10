using SGS.Application.DTOs;
using SGS.Application.Models;

namespace SGS.Application.Interfaces;

public interface IStudentsGroupService
{
    Task<Result<GroupDto?>> GetGroupByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result<IEnumerable<GroupDto>>> GetAllGroupsAsync(CancellationToken cancellationToken = default);
    Task<Result<GroupDto>> CreateGroupAsync(CreateStudentsGroupDto groupDtos, CancellationToken cancellationToken = default);
    Task<Result<GroupDto>> UpdateGroupAsync(Guid id, GroupDto groupDtos, CancellationToken cancellationToken = default);
    Task<Result<bool>> DeleteGroupAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result<GroupDto?>> GetByGroupByNameAsync(string name, CancellationToken cancellationToken = default);
    Task<Result<GroupDto?>> GetGroupByStudentIdAsync(Guid studentId, CancellationToken cancellationToken = default);
    Task<Result<IEnumerable<GroupDto>>> GetGroupByCourseIdAsync(Guid courseId, CancellationToken cancellationToken = default);
    Task<Result<bool>> AssignUserToGroupAsync(Guid userId, Guid groupId, CancellationToken cancellationToken = default);
}