using SGS.Application.Interfaces;
using SGS.Application.Models.DocumentProcessing;

namespace SGS.Application.Services.Document.Processing;

/// <summary>Processes document bytes independently of upload/storage and of grading.</summary>
public sealed class DocumentProcessor(
    IEnumerable<IStructuredDocumentTextExtractor> extractors,
    IDocumentChunker chunker) : IDocumentProcessor
{
    private const int MaxBytes = 32 * 1024 * 1024;

    public async Task<DocumentProcessingResult> ProcessAsync(
        string fileName, byte[] fileBytes, ChunkingOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fileName);
        ArgumentNullException.ThrowIfNull(fileBytes);
        
        if (fileBytes.Length == 0) throw new InvalidDataException("Empty document is not supported.");
        if (fileBytes.Length > MaxBytes)
            throw new InvalidDataException("Document exceeds the 32 MiB extraction limit.");

        options ??= new ChunkingOptions();
        options.Validate();
        cancellationToken.ThrowIfCancellationRequested();

        var extractor = extractors.FirstOrDefault(x => x.CanExtract(fileName))
            ?? throw new NotSupportedException($"Structured extraction is unavailable for {Path.GetExtension(fileName)}.");
        var document = await extractor.ExtractStructuredAsync(fileBytes, cancellationToken);
        cancellationToken.ThrowIfCancellationRequested();
        return new DocumentProcessingResult(document, chunker.Chunk(document, options));
    }
}
