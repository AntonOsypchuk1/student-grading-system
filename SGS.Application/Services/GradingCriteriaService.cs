using AutoMapper;
using Microsoft.Extensions.Logging;
using SGS.Application.DTOs;
using SGS.Application.Interfaces;
using SGS.Domain.Entities;
using SGS.Domain.Enums;
using SGS.Infrastructure.Data;
using SGS.Infrastructure.Repositories.Interfaces;
using SGS.Infrastructure.Repositories.UnitOfWork;

namespace SGS.Application.Services;

public class GradingCriteriaService(
    IUnitOfWork unitOfWork,
    ILogger<GradingCriteriaService> logger,
    IMapper mapper)
    : IGradingCriteriaService
{
    private readonly IGradingCriteriaRepository _criteriaRepository = unitOfWork.GradingCriteria;
    private readonly IUserRepository _userRepository = unitOfWork.Users;
    
    public async Task<GradingCriteriaDto> CreateCriteriaAsync
        (CreateGradingCriteriaCommand command, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Creating grading criteria for user: {UserId}", command.UserId);

        try
        {
            var user = await _userRepository.GetByIdAsync(command.UserId, cancellationToken);
            if (user is not { Role: UserRole.Teacher })
            {
                logger.LogWarning("Unauthorized attempt to create criteria by user: {UserId}", command.UserId);
                throw new Exception("Only teachers can create grading criteria.");
            }

            var criteria = new GradingCriteria
            {
                Id = Guid.NewGuid(),
                UserId = command.UserId,
                CriteriaDescription = command.CriteriaDescription,
                CreatedAt = DateTime.UtcNow
            };

            await _criteriaRepository.CreateAsync(criteria, cancellationToken);
            logger.LogInformation("Successfully created grading criteria with ID: {CriteriaId}", criteria.Id);

            return mapper.Map<GradingCriteriaDto>(criteria);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating grading criteria for user: {UserId}", command.UserId);
            throw;
        }
    }

    public async Task<GradingCriteriaDto?> GetCriteriaByIdAsync
        (Guid id, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Retrieving grading criteria with ID: {CriteriaId}", id);

        try
        {
            var criteria = await _criteriaRepository.GetByIdAsync(id, cancellationToken);

            if (criteria == null)
            {
                logger.LogWarning("Grading criteria not found with ID: {CriteriaId}", id);
                return null;
            }

            logger.LogInformation("Successfully retrieved grading criteria with ID: {CriteriaId}", id);
            return mapper.Map<GradingCriteriaDto>(criteria);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving grading criteria with ID: {CriteriaId}", id);
            throw;
        }
    }

    public async Task<IEnumerable<GradingCriteriaDto>> GetAllCriteriaAsync
        (CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Retrieving all grading criteria");

        try
        {
            var criteriaList = await _criteriaRepository.GetAllAsync(cancellationToken);
            logger.LogInformation("Successfully retrieved {Count} grading criteria", 
                criteriaList.Count());

            return mapper.Map<IEnumerable<GradingCriteriaDto>>(criteriaList);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving all grading criteria");
            throw;
        }
    }
}