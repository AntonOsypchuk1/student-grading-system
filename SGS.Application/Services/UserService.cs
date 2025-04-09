using AutoMapper;
using SGS.Application.DTOs;
using SGS.Application.Interfaces;
using SGS.Domain.Enums;
using SGS.Infrastructure.Repositories.Interfaces;

namespace SGS.Application.Services;

public class UserService(IUserRepository userRepository, IMapper mapper) : IUserService
{
    public async Task<UserDto?> GetUserByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var user = await userRepository.GetByEmailAsync(email, cancellationToken);
        return user == null ? null : mapper.Map<UserDto>(user);
    }

    public async Task<IEnumerable<UserDto>> GetTeachersAsync(CancellationToken cancellationToken = default)
    {
        var teachers = await userRepository.GetByRoleAsync(UserRole.Teacher, cancellationToken);
        return mapper.Map<IEnumerable<UserDto>>(teachers);
    }

    public async Task<IEnumerable<UserDto>> GetStudentsAsync(CancellationToken cancellationToken = default)
    {
        var students = await userRepository.GetByRoleAsync(UserRole.Student, cancellationToken);
        return mapper.Map<IEnumerable<UserDto>>(students);
    }

    public async Task<IEnumerable<UserDto>> GetStudentsByGroupAsync(Guid groupId, CancellationToken cancellationToken = default)
    {
        var groupStudents = await userRepository.GetStudentsByGroupAsync(groupId, cancellationToken);
        return mapper.Map<IEnumerable<UserDto>>(groupStudents);
    }
}