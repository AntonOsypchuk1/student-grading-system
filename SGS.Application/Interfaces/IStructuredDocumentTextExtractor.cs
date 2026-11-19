using SGS.Application.Models.DocumentProcessing;

namespace SGS.Application.Interfaces;

/// <summary>Extended extraction contract; the existing flat-text contract remains supported.</summary>
public interface IStructuredDocumentTextExtractor : IDocumentTextExtractor
{
    Task<ExtractedDocument> ExtractStructuredAsync(
        byte[] fileBytes, CancellationToken cancellationToken = default);
}
