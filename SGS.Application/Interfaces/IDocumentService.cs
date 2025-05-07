using SGS.Application.DTOs;
using SGS.Application.Helpers;

namespace SGS.Application.Interfaces;

public interface IDocumentService
{
    Task<Result<DocumentDto>> UploadDocumentAsync(UploadDocumentCommand command, CancellationToken cancellationToken = default);
    Task<Result<DocumentDto?>> GetDocumentByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Result<IEnumerable<DocumentDto>?>>
        GetDocumentsByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
}