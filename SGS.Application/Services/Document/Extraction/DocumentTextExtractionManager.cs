using Microsoft.Extensions.Logging;
using SGS.Application.Interfaces;

namespace SGS.Application.Services.Document.Extraction;

public class DocumentTextExtractionManager(
    IEnumerable<IDocumentTextExtractor> extractors,
    ILogger<DocumentTextExtractionManager> logger) 
    : IDocumentTextExtractionManager
{
    public async Task<string> ExtractTextAsync(string filePath, byte[] fileBytes,
        CancellationToken cancellationToken = default)
    {
        var extractor = extractors.FirstOrDefault(x => x.CanExtract(filePath));

        if (extractor is null)
        {
            logger.LogWarning("No suitable extractor found for file type: {FileType}", Path.GetExtension(filePath));
            throw new NotSupportedException($"Text extraction not supported for file type: {Path.GetExtension(filePath)}");
        }
        
        logger.LogInformation("Extracting text from {FilePath} using {ExtractorType}", 
            filePath, extractor.GetType().Name);

        return await extractor.ExtractTextAsync(fileBytes, cancellationToken);
    }
}