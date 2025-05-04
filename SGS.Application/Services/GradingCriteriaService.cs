using AutoMapper;
using Microsoft.Extensions.Logging;
using SGS.Application.DTOs;
using SGS.Application.Interfaces;
using SGS.Application.Services.Bases;
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
    : BaseService(unitOfWork, logger, mapper), IGradingCriteriaService
{
    public async Task<GradingCriteriaDto> CreateCriteriaAsync
        (CreateGradingCriteriaCommand command, CancellationToken cancellationToken = default)
    {
        return await ExecuteInTransactionAsync(async () =>
        {
            Logger.LogInformation("Creating grading criteria for user: {UserId}", command.UserId);

            try
            {
                var user = await UnitOfWork.Users.GetByIdAsync(command.UserId, cancellationToken);
                if (user is not { Role: UserRole.Teacher })
                {
                    Logger.LogWarning("Unauthorized attempt to create criteria by user: {UserId}", command.UserId);
                    throw new Exception("Only teachers can create grading criteria.");
                }

                var criteria = new GradingCriteria
                {
                    Id = Guid.NewGuid(),
                    UserId = command.UserId,
                    CriteriaDescription = command.CriteriaDescription,
                    CreatedAt = DateTime.UtcNow
                };

                await UnitOfWork.GradingCriteria.CreateAsync(criteria, cancellationToken);
                Logger.LogInformation("Successfully created grading criteria with ID: {CriteriaId}", criteria.Id);

                return Mapper.Map<GradingCriteriaDto>(criteria);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error creating grading criteria for user: {UserId}", command.UserId);
                throw;
            }
        }, cancellationToken);
    }

    public async Task<GradingCriteriaDto?> GetCriteriaByIdAsync
        (Guid id, CancellationToken cancellationToken = default)
    {
        Logger.LogInformation("Retrieving grading criteria with ID: {CriteriaId}", id);

        try
        {
            var criteria = await UnitOfWork.GradingCriteria.GetByIdAsync(id, cancellationToken);

            if (criteria == null)
            {
                Logger.LogWarning("Grading criteria not found with ID: {CriteriaId}", id);
                return null;
            }

            Logger.LogInformation("Successfully retrieved grading criteria with ID: {CriteriaId}", id);
            return Mapper.Map<GradingCriteriaDto>(criteria);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error retrieving grading criteria with ID: {CriteriaId}", id);
            throw;
        }
    }

    public async Task<IEnumerable<GradingCriteriaDto>> GetAllCriteriaAsync
        (CancellationToken cancellationToken = default)
    {
        Logger.LogInformation("Retrieving all grading criteria");

        try
        {
            var criteriaList = await UnitOfWork.GradingCriteria.GetAllAsync(cancellationToken);
            Logger.LogInformation("Successfully retrieved {Count} grading criteria", 
                criteriaList.Count());

            return Mapper.Map<IEnumerable<GradingCriteriaDto>>(criteriaList);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error retrieving all grading criteria");
            throw;
        }
    }
}