using SGS.Application.Models.DocumentProcessing;
using SGS.Application.Services.Document.Processing;

namespace SGS.Application.Interfaces;

public interface IDocumentProcessor
{
    Task<DocumentProcessingResult> ProcessAsync(
        string fileName, byte[] fileBytes, ChunkingOptions? options = null,
        CancellationToken cancellationToken = default);
}
