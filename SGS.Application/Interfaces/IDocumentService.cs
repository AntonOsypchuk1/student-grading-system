using SGS.Application.DTOs;
using SGS.Domain.Entities;

namespace SGS.Application.Interfaces;

public interface IDocumentService
{
    Task<DocumentDto> UploadDocumentAsync(UploadDocumentCommand command, CancellationToken cancellationToken = default);
    Task<DocumentDto?> GetDocumentByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<DocumentDto>> GetDocumentsByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
}