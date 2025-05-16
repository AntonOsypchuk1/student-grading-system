using System.Text;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
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

            for (var i = 1; i <= pdf.GetNumberOfPages(); i++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var page = pdf.GetPage(i);
                sb.Append(PdfTextExtractor.GetTextFromPage(page));
                sb.AppendLine();
            }

            return Task.FromResult(sb.ToString());
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error extracting text from PDF");
            throw;
        }
    }
}
