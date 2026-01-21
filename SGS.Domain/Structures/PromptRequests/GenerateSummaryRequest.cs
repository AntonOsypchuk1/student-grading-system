namespace SGS.Domain.Structures.PromptRequests;

public class GenerateSummaryRequest
{
    /// <summary>
    /// Combined results from all criteria evaluations
    /// </summary>
    public string EvaluationResult { get; set; } = string.Empty;
        
    /// <summary>
    /// Overall calculated score
    /// </summary>
    public double Score { get; set; }
        
    /// <summary>
    /// The maximum possible score (typically 100)
    /// </summary>
    public double MaxScore { get; set; } = 100;
}
