using AutoMapper;
using Microsoft.Extensions.Logging;
using SGS.Application.DTOs;
using SGS.Application.Extensions;
using SGS.Application.Interfaces;
using SGS.Application.Models;
using SGS.Application.Services.Document.Extraction;
using SGS.Infrastructure.Repositories.UnitOfWork;

namespace SGS.Application.Services.Document;

public class DocumentService(
    IDocumentTextExtractionManager extractionManager,
    IFileStorageService fileStorageService,
    IUnitOfWork unitOfWork,
    ILogger<DocumentService> logger,
    IMapper mapper)
    : BaseService(unitOfWork, logger, mapper), IDocumentService
{
    public async Task<Result<DocumentDto>> UploadDocumentAsync(UploadDocumentCommand command,
        CancellationToken cancellationToken = default)
    {
        return await ExecuteInTransactionAsync(async () =>
        {
            Logger.LogInformation("Uploading document: {FileName} for assignment: {AssignmentId}",
                command.FileName, command.AssignmentId);

            try
            {
                var assignment = await UnitOfWork.Assignments.GetByIdAsync(command.AssignmentId, cancellationToken);
                if (assignment == null)
                    return Result<DocumentDto>.Failure("Assignment not found");
                
                var filePath =
                    await fileStorageService.SaveFileAsync(command.FileName, command.FileContent, cancellationToken);
                Logger.LogInformation("File saved successfully at: {FilePath}", filePath);

                var document = Mapper.Map<Domain.Entities.Document>(command);
                document.Id = Guid.NewGuid();
                document.FilePath = filePath;
                document.UploadedAt = DateTime.UtcNow;

                await UnitOfWork.Documents.CreateAsync(document, cancellationToken);
                Logger.LogInformation("Document record created with ID: {DocumentId}", document.Id);

                return Result<DocumentDto>.Success(Mapper.Map<DocumentDto>(document));
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error uploading document: {FileName}", command.FileName);
                return Result<DocumentDto>.Failure($"Failed to upload document: {ex.Message}");
            }
        }, cancellationToken);
    }

    public async Task<Result<DocumentDto?>> GetDocumentByIdAsync(Guid id,
        CancellationToken cancellationToken = default)
    {
        Logger.LogInformation("Retrieving document with ID: {DocumentId}", id);

        try
        {
            var document = await UnitOfWork.Documents.GetByIdAsync(id, cancellationToken);

            if (document == null)
            {
                Logger.LogWarning("Document not found with ID: {DocumentId}", id);
                return Result<DocumentDto?>.Failure("Document not found.");
            }

            Logger.LogInformation("Successfully retrieved document with ID: {DocumentId}", id);
            return Result<DocumentDto?>.Success(Mapper.Map<DocumentDto>(document));
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error retrieving document with ID: {DocumentId}", id);
            return Result<DocumentDto?>.Failure($"Failed to retrieve document: {ex.Message}");
        }
    }

    public async Task<Result<byte[]>> GetDocumentContentBytesAsync(Guid id,
        CancellationToken cancellationToken = default)
    {
        Logger.LogInformation("Retrieving content for document with ID: {DocumentId}", id);

        try
        {
            var document = await UnitOfWork.Documents.GetByIdAsync(id, cancellationToken);

            if (document == null)
            {
                Logger.LogWarning("Document not found with ID: {DocumentId}", id);
                return Result<byte[]>.Failure("Document not found.");
            }

            var fileContent = await fileStorageService.GetFileAsync(document.FilePath, cancellationToken);
            Logger.LogInformation("Successfully retrieved content for document with ID: {DocumentId}", id);
        
            return Result<byte[]>.Success(fileContent);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error retrieving document content with ID: {DocumentId}", id);
            return Result<byte[]>.Failure($"Failed to retrieve document content: {ex.Message}");
        }
    }

    public async Task<Result<string>> GetDocumentContentTextAsync(Guid id, CancellationToken cancellationToken = default)
    {
        Logger.LogInformation("Retrieving content for document with ID: {DocumentId}", id);
        
        try
        {
            var document = await UnitOfWork.Documents.GetByIdAsync(id, cancellationToken);

            if (document == null)
            {
                Logger.LogWarning("Document not found with ID: {DocumentId}", id);
                return Result<string>.Failure("Document not found.");
            }
                
            var filePath = document.FilePath;
                
            var fileContent = await fileStorageService.GetFileTextAsync(filePath, extractionManager, cancellationToken);
            Logger.LogInformation("Successfully retrieved content for document with ID: {DocumentId}", id);

            return Result<string>.Success(fileContent);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error extracting text from document {DocumentId}", id);
            return Result<string>.Failure($"Failed to retrieve document content: {ex.Message}");
        }
    }

    public async Task<Result<IEnumerable<DocumentDto>?>> GetDocumentsByUserIdAsync(Guid userId,
        CancellationToken cancellationToken = default)
    {
        Logger.LogInformation("Retrieving documents for user with ID: {UserId}", userId);

        try
        {
            var documents = await UnitOfWork.Documents.GetDocumentsByUserIdAsync(userId, cancellationToken);
            Logger.LogInformation("Successfully retrieved {Count} documents for user: {UserId}",
                documents?.Count() ?? 0, userId);

            return Result<IEnumerable<DocumentDto>?>.Success(Mapper.Map<IEnumerable<DocumentDto>>(documents));
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error retrieving documents for user with ID: {UserId}", userId);
            return Result<IEnumerable<DocumentDto>?>.Failure($"Failed to retrieve documents: {ex.Message}");
        }
    }

    public async Task<Result<bool>> DeleteDocumentAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await ExecuteInTransactionAsync(async () =>
        {
            Logger.LogInformation("Deleting document with ID: {DocumentId}", id);

            try
            {
                var document = await UnitOfWork.Documents.GetByIdAsync(id, cancellationToken);

                if (document == null)
                {
                    Logger.LogWarning("Document not found with ID: {DocumentId}", id);
                    return Result<bool>.Failure("Document not found.");
                }
                
                await fileStorageService.DeleteFileAsync(document.FilePath, cancellationToken);
                Logger.LogInformation("File deleted successfully from path: {FilePath}", document.FilePath);
                
                UnitOfWork.Documents.Delete(document);
                Logger.LogInformation("Document record deleted with ID: {DocumentId}", document.Id);
                
                return Result<bool>.Success(true);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error deleting document with ID: {DocumentId}", id);
                return Result<bool>.Failure($"Failed to delete document: {ex.Message}");
            }
        }, cancellationToken);
    }
}