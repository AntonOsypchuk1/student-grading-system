namespace SGS.Domain.Structures.PromptResponses;

/// <summary>
/// Result of evaluating a single criterion
/// </summary>
public class CriterionResultResponse
{
    /// <summary>
    /// ID matching the criterion from the request
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Title of the criterion
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Numeric score assigned (0-100)
    /// </summary>
    public double Score { get; set; }

    /// <summary>
    /// Weight of this criterion (copied from request)
    /// </summary>
    public double Weight { get; set; }

    /// <summary>
    /// Detailed justification for the score
    /// </summary>
    public string Justification { get; set; } = string.Empty;

    /// <summary>
    /// Specific strengths identified in the submission for this criterion
    /// </summary>
    public List<string> Strengths { get; set; } = [];

    /// <summary>
    /// Areas needing improvement for this criterion
    /// </summary>
    public List<string> Improvements { get; set; } = [];
}
