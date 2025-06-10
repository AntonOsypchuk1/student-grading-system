using System.Text;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Canvas.Parser.Listener;
using Microsoft.Extensions.Logging;
using SGS.Application.Interfaces;

namespace SGS.Application.Services.Document.Extraction;

public class PdfDocumentTextExtractor(ILogger<PdfDocumentTextExtractor> logger) : IDocumentTextExtractor
{
    public bool CanExtract(string filePath)
    {
        return Path.GetExtension(filePath).Equals(".pdf", StringComparison.OrdinalIgnoreCase);
    }

    public Task<string> ExtractTextAsync(byte[] fileBytes, CancellationToken cancellationToken = default)
    {
        try
        {
            using var memoryStream = new MemoryStream(fileBytes);
            using var reader = new PdfReader(memoryStream);
            using var pdf = new PdfDocument(reader);

            var sb = new StringBuilder();
            var pageCount = pdf.GetNumberOfPages();
            logger.LogInformation("Processing PDF with {PageCount} pages", pageCount);

            for (var i = 1; i <= pageCount; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var page = pdf.GetPage(i);
            
                logger.LogInformation("Page {I} properties:", i);
                logger.LogInformation("- Page size: {GetPageSize}", page.GetPageSize());
                logger.LogInformation("- Rotation: {GetRotation}", page.GetRotation());
                logger.LogInformation("- Has content stream: {B}", page.GetContentStreamCount() > 0);
            
                var content = page.GetContentBytes();
                if (content is { Length: > 0 })
                {
                    logger.LogInformation("Content bytes length: {ContentLength}", content.Length);
                    var contentString = Encoding.UTF8.GetString(content);
                    logger.LogInformation("Raw content (first 100 bytes): {Substring}", contentString[..Math.Min(100, contentString.Length)]);
                }

                var pageResources = page.GetResources();
                if (pageResources != null)
                {
                    logger.LogInformation("- Resources found in page");
                }

                var strategy = new LocationTextExtractionStrategy();
                var pageText = PdfTextExtractor.GetTextFromPage(page, strategy);
                sb.Append(pageText);
                sb.AppendLine();
            }


            var result = sb.ToString();
            logger.LogInformation("Final extracted text length: {ResultLength}", result.Length);
            return Task.FromResult(result);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error extracting text from PDF");
            throw;
        }
    }
}
