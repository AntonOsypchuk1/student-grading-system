namespace SGS.Application.Interfaces;

public interface IAIService
{
    /// <summary>
    /// Analyzes the document at the given file path and returns a summary and a generated grade.
    /// </summary>
    /// <param name="filePath">The full path to the document file.</param>
    /// <param name="cancellationToken"></param>
    /// <returns>A tuple containing the generated summary and the generated grade.</returns>
    Task<(string summary, int generatedGrade)> AnalyzeDocumentAsync(string filePath, CancellationToken cancellationToken = default);
}