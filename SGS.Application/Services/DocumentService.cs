using AutoMapper;
using Microsoft.Extensions.Logging;
using SGS.Application.DTOs;
using SGS.Application.Interfaces;
using SGS.Application.Services.Bases;
using SGS.Domain.Entities;
using SGS.Infrastructure.Data;
using SGS.Infrastructure.Repositories.Interfaces;
using SGS.Infrastructure.Repositories.UnitOfWork;

namespace SGS.Application.Services;

public class DocumentService(
    IUnitOfWork unitOfWork,
    IFileStorageService fileStorageService,
    ILogger<DocumentService> logger,
    IMapper mapper)
    : BaseService(unitOfWork, logger, mapper), IDocumentService
{
    public async Task<DocumentDto> UploadDocumentAsync(UploadDocumentCommand command,
        CancellationToken cancellationToken = default)
    {
        return await ExecuteInTransactionAsync(async () =>
        {
            Logger.LogInformation("Uploading document: {FileName} for assignment: {AssignmentId}",
                command.FileName, command.AssignmentId);

            try
            {
                var filePath =
                    await fileStorageService.SaveFileAsync(command.FileName, command.FileContent, cancellationToken);
                Logger.LogInformation("File saved successfully at: {FilePath}", filePath);

                var document = Mapper.Map<Document>(command);
                document.Id = Guid.NewGuid();
                document.FilePath = filePath;
                document.UploadedAt = DateTime.UtcNow;

                await UnitOfWork.Documents.CreateAsync(document, cancellationToken);
                Logger.LogInformation("Document record created with ID: {DocumentId}", document.Id);

                return Mapper.Map<DocumentDto>(document);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error uploading document: {FileName}", command.FileName);
                throw;
            }
        }, cancellationToken);
    }

    public async Task<DocumentDto?> GetDocumentByIdAsync(Guid id,
        CancellationToken cancellationToken = default)
    {
        Logger.LogInformation("Retrieving document with ID: {DocumentId}", id);

        try
        {
            var document = await UnitOfWork.Documents.GetByIdAsync(id, cancellationToken);

            if (document == null)
            {
                Logger.LogWarning("Document not found with ID: {DocumentId}", id);
                return null;
            }

            Logger.LogInformation("Successfully retrieved document with ID: {DocumentId}", id);
            return Mapper.Map<DocumentDto>(document);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error retrieving document with ID: {DocumentId}", id);
            throw;
        }
    }

    public async Task<IEnumerable<DocumentDto>?> GetDocumentsByUserIdAsync(Guid userId,
        CancellationToken cancellationToken = default)
    {
        Logger.LogInformation("Retrieving documents for user with ID: {UserId}", userId);

        try
        {
            var documents = await UnitOfWork.Documents.GetDocumentsByUserIdAsync(userId, cancellationToken);
            Logger.LogInformation("Successfully retrieved {Count} documents for user: {UserId}",
                documents?.Count() ?? 0, userId);

            return Mapper.Map<IEnumerable<DocumentDto>>(documents);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error retrieving documents for user with ID: {UserId}", userId);
            throw;
        }
    }
}