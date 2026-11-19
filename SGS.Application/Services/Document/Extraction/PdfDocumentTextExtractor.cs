using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Canvas.Parser.Listener;
using Microsoft.Extensions.Logging;
using ProcessingFormat = SGS.Application.Models.DocumentProcessing.DocumentFormat;
using SGS.Application.Interfaces;
using SGS.Application.Models.DocumentProcessing;

namespace SGS.Application.Services.Document.Extraction;

/// <summary>
/// Uses the existing iText dependency for both legacy text and page-aware extraction.
/// </summary>
public class PdfDocumentTextExtractor(ILogger<PdfDocumentTextExtractor> logger)
    : IStructuredDocumentTextExtractor
{
    public bool CanExtract(string filePath) =>
        Path.GetExtension(filePath).Equals(".pdf", StringComparison.OrdinalIgnoreCase);

    // Preserve the old contract for the existing DocumentService and its consumers.
    public async Task<string> ExtractTextAsync(byte[] fileBytes, CancellationToken cancellationToken = default)
    {
        var document = await ExtractStructuredAsync(fileBytes, cancellationToken);
        return string.Join(Environment.NewLine, document.Sources.Select(s => s.Text)) + Environment.NewLine;
    }

    public Task<ExtractedDocument> ExtractStructuredAsync(
        byte[] fileBytes, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(fileBytes);
        var sources = new List<DocumentSource>();
        var warnings = new List<string>();
        try
        {
            using var stream = new MemoryStream(fileBytes, writable: false);
            using var reader = new PdfReader(stream);
            using var pdf = new PdfDocument(reader);
            logger.LogInformation("Extracting text from {PageCount} PDF pages", pdf.GetNumberOfPages());

            for (int pageNumber = 1; pageNumber <= pdf.GetNumberOfPages(); pageNumber++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var page = pdf.GetPage(pageNumber);
                var text = ExtractionTextUtilities.Normalize(
                    PdfTextExtractor.GetTextFromPage(page, new LocationTextExtractionStrategy()));
                var hasImages = ContainsImages(page.GetResources().GetPdfObject());
                var sourceId = $"pdf-page-{pageNumber:D4}";
                var empty = string.IsNullOrWhiteSpace(text);
                sources.Add(new DocumentSource(sourceId, pageNumber, text,
                    empty ? SourceExtractionStatus.EmptyOrUnreadable : SourceExtractionStatus.Extracted,
                    hasImages));

                if (empty)
                    warnings.Add($"PDF page {pageNumber} contains no extractable text; OCR or manual review may be needed.");
                if (hasImages)
                    warnings.Add($"PDF page {pageNumber} contains images that were not interpreted.");
            }
        }
        catch (OperationCanceledException) { throw; }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to extract structured PDF text");
            throw;
        }

        var (cleanedSources, cleanedBlocks) = PdfBoilerplateCleaner.Clean(sources, warnings);
        return Task.FromResult(ExtractionTextUtilities.Build(
            ExtractionTextUtilities.DocumentId(fileBytes),
            ProcessingFormat.Pdf,
            cleanedSources,
            cleanedBlocks,
            warnings));
    }

    // Inspect XObjects (including nested Form resources) rather than logging raw PDF streams.
    private static bool ContainsImages(PdfDictionary? resources, int depth = 0)
    {
        if (resources is null || depth > 8) return false;
        var objects = resources.GetAsDictionary(PdfName.XObject);
        if (objects is null) return false;
        foreach (var key in objects.KeySet())
        {
            var stream = objects.GetAsStream(key);
            if (stream is null) continue;
            var subtype = stream.GetAsName(PdfName.Subtype);
            if (PdfName.Image.Equals(subtype)) return true;
            if (PdfName.Form.Equals(subtype) &&
                ContainsImages(stream.GetAsDictionary(PdfName.Resources), depth + 1))
                return true;
        }
        return false;
    }
}
