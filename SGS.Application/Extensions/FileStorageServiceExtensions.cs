using SGS.Application.Interfaces;
using SGS.Application.Services.Document.Extraction;

namespace SGS.Application.Extensions;

public static class FileStorageServiceExtensions
{
    /// <summary>
    /// Gets the text content of a file using the document text extraction manager
    /// </summary>
    public static async Task<string> GetFileTextAsync(
        this IFileStorageService fileStorage,
        string filePath,
        IDocumentTextExtractionManager extractionManager,
        CancellationToken cancellationToken = default)
    {
        var fileBytes = await fileStorage.GetFileAsync(filePath, cancellationToken);
            
        return await extractionManager.ExtractTextAsync(filePath, fileBytes, cancellationToken);
    }
}