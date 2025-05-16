namespace SGS.Application.Interfaces;

public interface IDocumentTextExtractionManager
{
    Task<string> ExtractTextAsync(string filePath, byte[] fileBytes, CancellationToken cancellationToken = default);
}