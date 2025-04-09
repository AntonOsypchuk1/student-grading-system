using AutoMapper;
using Microsoft.Extensions.Logging;
using SGS.Application.DTOs;
using SGS.Application.Interfaces;
using SGS.Domain.Entities;
using SGS.Infrastructure.Repositories.Interfaces;

namespace SGS.Application.Services;

public class DocumentService(
    IDocumentRepository documentRepository,
    IFileStorageService fileStorageService,
    ILogger<DocumentService> logger,
    IMapper mapper)
    : IDocumentService
{
    public async Task<DocumentDto> UploadDocumentAsync(UploadDocumentCommand command,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Uploading document: {FileName} for assignment: {AssignmentId}",
            command.FileName, command.AssignmentId);

        try
        {
            var filePath = await fileStorageService.SaveFileAsync(command.FileName, command.FileContent, cancellationToken);
            logger.LogInformation("File saved successfully at: {FilePath}", filePath);

            var document = mapper.Map<Document>(command);
            document.Id = Guid.NewGuid();
            document.FilePath = filePath;
            document.UploadedAt = DateTime.UtcNow;

            await documentRepository.CreateAsync(document, cancellationToken);
            logger.LogInformation("Document record created with ID: {DocumentId}", document.Id);

            return mapper.Map<DocumentDto>(document);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error uploading document: {FileName}", command.FileName);
            throw;
        }
    }

    public async Task<DocumentDto?> GetDocumentByIdAsync(Guid id,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Retrieving document with ID: {DocumentId}", id);

        try
        {
            var document = await documentRepository.GetByIdAsync(id, cancellationToken);

            if (document == null)
            {
                logger.LogWarning("Document not found with ID: {DocumentId}", id);
                return null;
            }

            logger.LogInformation("Successfully retrieved document with ID: {DocumentId}", id);
            return mapper.Map<DocumentDto>(document);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving document with ID: {DocumentId}", id);
            throw;
        }
    }

    public async Task<IEnumerable<DocumentDto>?> GetDocumentsByUserIdAsync(Guid userId,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Retrieving documents for user with ID: {UserId}", userId);

        try
        {
            var documents = await documentRepository.GetDocumentsByUserIdAsync(userId, cancellationToken);
            logger.LogInformation("Successfully retrieved {Count} documents for user: {UserId}",
                documents?.Count() ?? 0, userId);

            return mapper.Map<IEnumerable<DocumentDto>>(documents);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving documents for user with ID: {UserId}", userId);
            throw;
        }
    }
}