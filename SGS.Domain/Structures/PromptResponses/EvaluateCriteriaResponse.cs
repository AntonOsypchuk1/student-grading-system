namespace SGS.Domain.Structures.PromptResponses;

/// <summary>
/// Response model for the batch criteria evaluation
/// </summary>
public class EvaluateCriteriaResponse
{
    /// <summary>
    /// Collection of individual criterion evaluation results
    /// </summary>
    public List<CriterionResultResponse> CriteriaResults { get; set; } = [];
}
