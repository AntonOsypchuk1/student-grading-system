using AutoMapper;
using SGS.Application.DTOs;
using SGS.Application.Interfaces;
using SGS.Domain.Entities;
using SGS.Infrastructure.Repositories.Interfaces;

namespace SGS.Application.Services;

public class DocumentService(
    IDocumentRepository documentRepository,
    IMapper mapper,
    IFileStorageService fileStorageService)
    : IDocumentService
{
    public async Task<DocumentDto> UploadDocumentAsync(UploadDocumentCommand command,
        CancellationToken cancellationToken = default)
    {
        var filePath = await fileStorageService.SaveFileAsync(command.FileName, command.FileContent, cancellationToken);

        var document = mapper.Map<Document>(command);
        document.Id = Guid.NewGuid();
        document.FilePath = filePath;
        document.UploadedAt = DateTime.UtcNow;

        await documentRepository.CreateAsync(document, cancellationToken);
        return mapper.Map<DocumentDto>(document);
    }

    public async Task<DocumentDto?> GetDocumentByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var document = await documentRepository.GetByIdAsync(id, cancellationToken);
        return document == null ? null : mapper.Map<DocumentDto>(document);
    }

    public async Task<IEnumerable<DocumentDto>?> GetDocumentsByUserIdAsync(Guid userId,
        CancellationToken cancellationToken = default)
    {
        var documents = await documentRepository.GetDocumentsByUserIdAsync(userId, cancellationToken);
        return mapper.Map<IEnumerable<DocumentDto>>(documents);
    }
}