namespace SGS.Domain.Structures.PromptResponses;

/// <summary>
/// Response model for the summary generation
/// </summary>
public class GenerateSummaryResponse
{
    /// <summary>
    /// Comprehensive summary of the evaluation
    /// </summary>
    public string Summary { get; set; } = string.Empty;
        
    /// <summary>
    /// Key strengths highlighted in the summary
    /// </summary>
    public List<string> Strengths { get; set; } = [];
        
    /// <summary>
    /// Key areas for improvement highlighted in the summary
    /// </summary>
    public List<string> Improvements { get; set; } = [];
}
