using AutoMapper;
using SGS.Application.DTOs;
using SGS.Application.Interfaces;
using SGS.Domain.Entities;
using SGS.Domain.Enums;
using SGS.Infrastructure.Repositories.Interfaces;

namespace SGS.Application.Services;

public class GradingCriteriaService(
    IGradingCriteriaRepository criteriaRepository,
    IUserRepository userRepository,
    IMapper mapper)
    : IGradingCriteriaService
{
    public async Task<GradingCriteriaDto> CreateCriteriaAsync(CreateGradingCriteriaCommand command, CancellationToken cancellationToken = default)
    {
        var user = await userRepository.GetByIdAsync(command.UserId, cancellationToken);
        if (user is not { Role: UserRole.Teacher })
            throw new Exception("Only teachers can create grading criteria.");
        
        var criteria = new GradingCriteria
        {
            Id = Guid.NewGuid(),
            UserId = command.UserId,
            CriteriaDescription = command.CriteriaDescription,
            CreatedAt = DateTime.UtcNow
        };
        await criteriaRepository.CreateAsync(criteria, cancellationToken);
        return mapper.Map<GradingCriteriaDto>(criteria);
    }

    public async Task<GradingCriteriaDto?> GetCriteriaByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var criteria = await criteriaRepository.GetByIdAsync(id, cancellationToken);
        return criteria == null ? null : mapper.Map<GradingCriteriaDto>(criteria);
    }

    public async Task<IEnumerable<GradingCriteriaDto>> GetAllCriteriaAsync(CancellationToken cancellationToken = default)
    {
        var criteriaList = await criteriaRepository.GetAllAsync(cancellationToken);
        return mapper.Map<IEnumerable<GradingCriteriaDto>>(criteriaList);
    }
}