using SGS.Application.DTOs;
using SGS.Application.Models;

namespace SGS.Application.Interfaces;

public interface IDocumentService
{
    Task<Result<DocumentDto>> UploadDocumentAsync(UploadDocumentCommand command, CancellationToken cancellationToken = default);
    
    Task<Result<DocumentDto?>> GetDocumentByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Result<IEnumerable<DocumentDto>>> GetDocumentsByUserIdAndAssignmentIdAsync(Guid userId, Guid assignmentId,
        CancellationToken cancellationToken = default);
    
    Task<Result<IEnumerable<DocumentDto>>> GetDocumentsByAssignmentIdAsync(Guid assignmentId,
        CancellationToken cancellationToken = default);

    Task<Result<DocumentDto?>> GetDocumentByUserIdAndAssignmentIdAsync(Guid userId, Guid assignmentId,
        CancellationToken cancellationToken = default);
    
    Task<Result<byte[]>> GetDocumentContentBytesAsync(Guid id, CancellationToken cancellationToken = default);
    
    Task<Result<string>> GetDocumentContentTextAsync(Guid id, CancellationToken cancellationToken = default);
    
    Task<Result<IEnumerable<DocumentDto>?>> GetDocumentsByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    
    Task<Result<bool>> DeleteDocumentAsync(Guid id, CancellationToken cancellationToken = default);
}