using SGS.Domain.Entities;

namespace SGS.Application.Interfaces;

public interface IAIService
{
    /// <summary>
    ///     Analyzes the document at the given file path and returns a summary and a generated grade.
    /// </summary>
    /// <param name="document">The document entity to analyze.</param>
    /// <param name="criteria">The grading criteria to use for the analysis.</param>   
    /// <param name="cancellationToken"></param>
    /// <returns>A tuple containing the generated summary and the generated grade.</returns>
    Task<(string Summary, int GeneratedGrade)> AnalyzeDocumentAsync(
        Document document,
        List<GradingCriteria> criteria,
        CancellationToken cancellationToken = default);
}