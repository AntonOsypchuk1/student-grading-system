namespace SGS.Application.Interfaces;

public interface IDocumentTextExtractor
{
    /// <summary>
    /// Checks if this extractor can handle the given file type
    /// </summary>
    bool CanExtract(string filePath);
    
    /// <summary>
    /// Extracts text from the document bytes.
    /// </summary>
    Task<string> ExtractTextAsync(byte[] fileBytes, CancellationToken cancellationToken = default);
}