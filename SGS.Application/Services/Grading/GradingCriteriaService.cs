using AutoMapper;
using Microsoft.Extensions.Logging;
using SGS.Application.DTOs;
using SGS.Application.Interfaces;
using SGS.Application.Models;
using SGS.Domain.Entities;
using SGS.Domain.Enums;
using SGS.Infrastructure.Repositories.UnitOfWork;

namespace SGS.Application.Services.Grading;

public class GradingCriteriaService(
    IUnitOfWork unitOfWork,
    ILogger<GradingCriteriaService> logger,
    IMapper mapper)
    : BaseService(unitOfWork, logger, mapper), IGradingCriteriaService
{
    public async Task<Result<GradingCriteriaDto>> CreateCriteriaAsync
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

                return Result<GradingCriteriaDto>.Success(Mapper.Map<GradingCriteriaDto>(criteria));
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error creating grading criteria for user: {UserId}", command.UserId);
                return Result<GradingCriteriaDto>.Failure($"Failed to create grading criteria: {ex.Message}");
            }
        }, cancellationToken);
    }

    public async Task<Result<GradingCriteriaDto?>> GetCriteriaByIdAsync
        (Guid id, CancellationToken cancellationToken = default)
    {
        Logger.LogInformation("Retrieving grading criteria with ID: {CriteriaId}", id);

        try
        {
            var criteria = await UnitOfWork.GradingCriteria.GetByIdAsync(id, cancellationToken);

            if (criteria == null)
            {
                Logger.LogWarning("Grading criteria not found with ID: {CriteriaId}", id);
                return Result<GradingCriteriaDto?>.Success(null);
            }

            Logger.LogInformation("Successfully retrieved grading criteria with ID: {CriteriaId}", id);
            return Result<GradingCriteriaDto?>.Success(Mapper.Map<GradingCriteriaDto>(criteria));
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error retrieving grading criteria with ID: {CriteriaId}", id);
            return Result<GradingCriteriaDto?>.Failure($"Failed to retrieve grading criteria: {ex.Message}");
        }
    }

    public async Task<Result<IEnumerable<GradingCriteriaDto>>> GetAllCriteriaAsync
        (CancellationToken cancellationToken = default)
    {
        Logger.LogInformation("Retrieving all grading criteria");

        try
        {
            var criteriaList = await UnitOfWork.GradingCriteria.GetAllAsync(cancellationToken);
            Logger.LogInformation("Successfully retrieved {Count} grading criteria", 
                criteriaList.Count());

            return Result<IEnumerable<GradingCriteriaDto>>.Success(Mapper.Map<IEnumerable<GradingCriteriaDto>>(criteriaList));
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error retrieving all grading criteria");
            return Result<IEnumerable<GradingCriteriaDto>>.Failure($"Failed to retrieve grading criteria: {ex.Message}");
        }
    }
}