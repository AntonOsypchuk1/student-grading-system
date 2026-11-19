using System.Security.Cryptography;
using SGS.Application.Common.Options;
using SGS.Application.Interfaces;
using SGS.Application.Models.DocumentProcessing;
using SGS.Application.Services.Document.Extraction;

namespace SGS.Application.Services.Document.Processing;

/// <summary>Processes document bytes independently of upload/storage and of grading.</summary>
public sealed class DocumentProcessor(
    IEnumerable<IStructuredDocumentTextExtractor> extractors,
    IDocumentChunker chunker,
    IDocumentImageExtractor? imageExtractor = null,
    IImageOcrProvider? ocrProvider = null) : IDocumentProcessor, IDisposable
{
    private const int MaxBytes = 32 * 1024 * 1024;
    private readonly IDocumentImageExtractor _imageExtractor = imageExtractor ?? new DocumentImageExtractor();
    private readonly IImageOcrProvider _ocrProvider = ocrProvider ?? new TesseractImageOcrProvider(new TesseractOcrOptions());
    private readonly bool _ownsOcrProvider = ocrProvider is null;

    public void Dispose()
    {
        if (_ownsOcrProvider && _ocrProvider is IDisposable disposable)
            disposable.Dispose();
    }

    public Task<DocumentProcessingResult> ProcessAsync(
        string fileName, byte[] fileBytes, ChunkingOptions? options = null,
        CancellationToken cancellationToken = default) =>
        ProcessCoreAsync(fileName, fileBytes, options, false, cancellationToken);

    public Task<DocumentProcessingResult> ProcessWithOcrAsync(
        string fileName, byte[] fileBytes, ChunkingOptions? options = null,
        CancellationToken cancellationToken = default) =>
        ProcessCoreAsync(fileName, fileBytes, options, true, cancellationToken);

    private async Task<DocumentProcessingResult> ProcessCoreAsync(
        string fileName, byte[] fileBytes, ChunkingOptions? options, bool recognizeImages,
        CancellationToken cancellationToken)
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
        document = _imageExtractor.ExtractImages(fileBytes, document, cancellationToken);
        if (recognizeImages)
            document = await AddOcrEvidenceAsync(document, cancellationToken);
        cancellationToken.ThrowIfCancellationRequested();
        return new DocumentProcessingResult(document, chunker.Chunk(document, options));
    }

    private async Task<ExtractedDocument> AddOcrEvidenceAsync(ExtractedDocument document,
        CancellationToken cancellationToken)
    {
        var sources = document.Sources.ToList();
        var blocks = document.Blocks.ToList();
        var warnings = document.Warnings.ToList();
        var results = new List<ImageOcrResult>();
        var cache = new Dictionary<string, OcrRecognition>(StringComparer.Ordinal);

        foreach (var image in document.Images)
        {
            cancellationToken.ThrowIfCancellationRequested();
            OcrRecognition recognition;
            if (image.IsLikelyDecorative)
            {
                recognition = new(OcrStatus.Skipped, Reason: "Likely decorative masked PDF image.");
            }
            else
            {
                // Reuse OCR for identical image bytes within a submission, but preserve
                // every occurrence's identity, section and page references.
                string hash = Convert.ToHexString(SHA256.HashData(image.Bytes));
                string key = $"{image.ContentType}:{hash}";
                if (!cache.TryGetValue(key, out var cached))
                {
                    cached = await _ocrProvider.RecognizeAsync(image, cancellationToken);
                    cache.Add(key, cached);
                }
                recognition = cached;
            }

            string text = recognition.Status == OcrStatus.Recognized
                ? ExtractionTextUtilities.Normalize(recognition.Text).Trim() : "";
            var status = recognition.Status == OcrStatus.Recognized && text.Length == 0
                ? OcrStatus.NoText : recognition.Status;
            results.Add(new ImageOcrResult(image.ImageId, status, text, recognition.Reason)
            {
                MeanConfidence = status == OcrStatus.Recognized ? recognition.MeanConfidence : null
            });
            if (status != OcrStatus.Recognized)
            {
                if (status == OcrStatus.Failed)
                    warnings.Add($"OCR failed for {image.ImageId}: {recognition.Reason}");
                continue;
            }

            string sourceId = $"ocr-{image.ImageId}";
            sources.Add(new DocumentSource(sourceId, image.PageNumber, text,
                SourceExtractionStatus.Extracted) { ImageId = image.ImageId });
            blocks.Add(new DocumentBlock($"ocr-{image.ImageId}", DocumentContentKind.OcrText,
                text, image.SectionPath, sourceId, 0));
        }

        // OCR provides text, not full image understanding; never declare an
        // image-bearing document complete merely because some OCR succeeded.
        var statusAfterOcr = blocks.Count == 0 ? ExtractionStatus.Unreadable :
            document.Images.Count > 0 ? ExtractionStatus.Partial : document.Status;
        return document with
        {
            Sources = sources,
            Blocks = blocks,
            OcrResults = results,
            Warnings = warnings,
            Status = statusAfterOcr
        };
    }
}
